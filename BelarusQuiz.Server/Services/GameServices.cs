using System.Collections.Concurrent;
using BelarusQuiz.Shared.Enums;
using BelarusQuiz.Shared.Models;

namespace BelarusQuiz.Server.Services;

// ─── Внутренние модели сервера ───────────────────────────────────────────────

public class ServerPlayer
{
    public string ConnectionId { get; set; } = "";
    public string Nickname { get; set; } = "";
    public int Score { get; set; }
    public bool IsReady { get; set; }
    public int Streak { get; set; }
    public int? LastAnswerIndex { get; set; }
    public long? AnswerTimeMs { get; set; }
}

public class ServerRoom
{
    public string Code { get; set; } = "";
    public GameMode Mode { get; set; }
    public QuizCategory Category { get; set; } = QuizCategory.All; // НОВОЕ
    public int MaxRounds { get; set; } = 10;
    public int TimerSeconds { get; set; } = 15;
    public RoomStatus Status { get; set; } = RoomStatus.Waiting;
    public List<ServerPlayer> Players { get; set; } = new();
    public int CurrentRound { get; set; }
    public List<(QuestionDto Question, int CorrectIndex, string Explanation)> Questions { get; set; } = new();
    public CancellationTokenSource? TimerCts { get; set; }
    public string HostConnectionId { get; set; } = "";
    public bool IsEvaluating { get; set; } = false; // НОВОЕ: защита от двойного вызова
}

// ─── RoomService ─────────────────────────────────────────────────────────────

public class RoomService
{
    private readonly ConcurrentDictionary<string, ServerRoom> _rooms = new();
    private readonly ConcurrentDictionary<string, string> _connectionToRoom = new();

    // НОВОЕ: добавлен параметр category
    public ServerRoom CreateRoom(string hostConnId, string nickname, GameMode mode,
        int rounds, int timer, QuizCategory category = QuizCategory.All)
    {
        var code = GenerateCode();
        var host = new ServerPlayer { ConnectionId = hostConnId, Nickname = nickname };
        var room = new ServerRoom
        {
            Code = code,
            Mode = mode,
            Category = category,
            MaxRounds = rounds,
            TimerSeconds = timer,
            HostConnectionId = hostConnId,
            Players = new() { host }
        };
        _rooms[code] = room;
        _connectionToRoom[hostConnId] = code;
        return room;
    }

    public (ServerRoom? room, string? error) JoinRoom(string connId, string nickname, string code)
    {
        code = code.ToUpper();
        if (!_rooms.TryGetValue(code, out var room))
            return (null, "Комната не найдена");
        if (room.Status != RoomStatus.Waiting)
            return (null, "Игра уже началась");
        if (room.Players.Count >= 4)
            return (null, "Комната заполнена");
        if (room.Players.Any(p => p.Nickname == nickname))
            return (null, "Имя уже занято");

        var player = new ServerPlayer { ConnectionId = connId, Nickname = nickname };
        room.Players.Add(player);
        _connectionToRoom[connId] = code;
        return (room, null);
    }

    public ServerRoom? GetRoomByConnection(string connId)
    {
        if (!_connectionToRoom.TryGetValue(connId, out var code)) return null;
        _rooms.TryGetValue(code, out var r);
        return r;
    }

    public ServerRoom? GetRoom(string code) =>
        _rooms.TryGetValue(code.ToUpper(), out var r) ? r : null;

    public void RemovePlayer(string connId)
    {
        if (!_connectionToRoom.TryRemove(connId, out var code)) return;
        if (!_rooms.TryGetValue(code, out var room)) return;
        room.Players.RemoveAll(p => p.ConnectionId == connId);
        if (room.Players.Count == 0)
        {
            room.TimerCts?.Cancel();
            _rooms.TryRemove(code, out _);
        }
    }

    public void SetReady(string connId, bool isReady)
    {
        var room = GetRoomByConnection(connId);
        var player = room?.Players.FirstOrDefault(p => p.ConnectionId == connId);
        if (player != null) player.IsReady = isReady;
    }

    public bool AllReady(string code)
    {
        var room = GetRoom(code);
        return room != null && room.Players.Count >= 1 && room.Players.All(p => p.IsReady);
    }

    public RoomInfo ToRoomInfo(ServerRoom room) => new()
    {
        Code = room.Code,
        Mode = room.Mode,
        Category = room.Category, // НОВОЕ
        MaxRounds = room.MaxRounds,
        TimerSeconds = room.TimerSeconds,
        Status = room.Status,
        Players = room.Players.Select(p => new PlayerInfo
        {
            Id = p.ConnectionId,
            Nickname = p.Nickname,
            Score = p.Score,
            IsReady = p.IsReady,
            Streak = p.Streak
        }).ToList()
    };

    private static string GenerateCode()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var rng = new Random();
        return new string(Enumerable.Range(0, 6).Select(_ => chars[rng.Next(chars.Length)]).ToArray());
    }
}

// ─── ScoreService ─────────────────────────────────────────────────────────────

public class ScoreService
{
    public int Calculate(bool correct, long timeMs, int timerSeconds, int streak)
    {
        if (!correct) return 0;
        double ratio = Math.Max(0, 1.0 - timeMs / (timerSeconds * 1000.0));
        int speedBonus = (int)(ratio * 50);
        int streakBonus = Math.Min(streak * 10, 50);
        return 100 + speedBonus + streakBonus;
    }
}