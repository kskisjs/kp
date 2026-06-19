using Microsoft.AspNetCore.SignalR;
using BelarusQuiz.Server.Data;
using BelarusQuiz.Server.Services;
using BelarusQuiz.Shared.Enums;
using BelarusQuiz.Shared.Models;

namespace BelarusQuiz.Server.Hubs;

public class GameHub : Hub
{
    private readonly RoomService _rooms;
    private readonly ScoreService _score;
    private readonly ILogger<GameHub> _log;

    public GameHub(RoomService rooms, ScoreService score, ILogger<GameHub> log)
    {
        _rooms = rooms;
        _score = score;
        _log = log;
    }

    // ── Подключение / отключение ──────────────────────────────────────────────

    public override async Task OnDisconnectedAsync(Exception? ex)
    {
        var room = _rooms.GetRoomByConnection(Context.ConnectionId);
        if (room != null)
        {
            var player = room.Players.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
            _rooms.RemovePlayer(Context.ConnectionId);
            if (player != null)
                await Clients.Group(room.Code).SendAsync(HubEvents.PlayerLeft, player.Nickname);
        }
        await base.OnDisconnectedAsync(ex);
    }

    // ── Создать комнату ───────────────────────────────────────────────────────

    public async Task CreateRoom(string nickname, int rounds = 10, int timer = 15)
    {
        nickname = nickname.Trim();
        if (string.IsNullOrEmpty(nickname)) { await SendError("Введите никнейм"); return; }
        var room = _rooms.CreateRoom(Context.ConnectionId, nickname, GameMode.Battle, rounds, timer);
        await Groups.AddToGroupAsync(Context.ConnectionId, room.Code);
        await Clients.Caller.SendAsync("RoomCreated", _rooms.ToRoomInfo(room));
        _log.LogInformation("Room {Code} created by {Nick}", room.Code, nickname);
    }

    // ── Войти в комнату ───────────────────────────────────────────────────────

    public async Task JoinRoom(string roomCode, string nickname)
    {
        nickname = nickname.Trim();
        if (string.IsNullOrEmpty(nickname)) { await SendError("Введите никнейм"); return; }
        var (room, error) = _rooms.JoinRoom(Context.ConnectionId, nickname, roomCode);
        if (error != null) { await SendError(error); return; }

        await Groups.AddToGroupAsync(Context.ConnectionId, room!.Code);
        await Clients.Group(room.Code).SendAsync(HubEvents.PlayerJoined, _rooms.ToRoomInfo(room));
        _log.LogInformation("{Nick} joined room {Code}", nickname, room.Code);
    }

    // ── Готов / не готов ──────────────────────────────────────────────────────

    public async Task SetReady(bool isReady)
    {
        var room = _rooms.GetRoomByConnection(Context.ConnectionId);
        if (room == null) return;
        _rooms.SetReady(Context.ConnectionId, isReady);
        await Clients.Group(room.Code).SendAsync(HubEvents.PlayerReadyChanged, _rooms.ToRoomInfo(room));

        if (_rooms.AllReady(room.Code) && room.Status == RoomStatus.Waiting)
            await StartGame(room.Code);
    }

    // ── Начать игру ───────────────────────────────────────────────────────────

    private async Task StartGame(string code)
    {
        var room = _rooms.GetRoom(code);
        if (room == null) return;
        room.Status = RoomStatus.Playing;
        room.Questions = QuestionBank.GetRandomSet(room.MaxRounds);
        room.CurrentRound = 0;
        foreach (var p in room.Players) { p.Score = 0; p.Streak = 0; }

        await Clients.Group(code).SendAsync(HubEvents.GameStarted, _rooms.ToRoomInfo(room));
        await Task.Delay(1000);
        await SendNextQuestion(code);
    }

    // ── Отправить следующий вопрос ────────────────────────────────────────────

    private async Task SendNextQuestion(string code)
    {
        var room = _rooms.GetRoom(code);
        if (room == null || room.CurrentRound >= room.MaxRounds)
        {
            await FinishGame(code);
            return;
        }

        var (q, _, _) = room.Questions[room.CurrentRound];
        var dto = new QuestionDto
        {
            Id = q.Id,
            Text = q.Text,
            Category = q.Category,
            Options = q.Options,
            RoundNumber = room.CurrentRound + 1,
            TotalRounds = room.MaxRounds
        };

        // Сброс ответов
        foreach (var p in room.Players) { p.LastAnswerIndex = null; p.AnswerTimeMs = null; }

        await Clients.Group(code).SendAsync(HubEvents.QuestionReceived, dto);

        // Запуск таймера
        room.TimerCts?.Cancel();
        room.TimerCts = new CancellationTokenSource();
        _ = RunTimer(code, room.TimerSeconds, room.TimerCts.Token);
    }

    private async Task RunTimer(string code, int seconds, CancellationToken ct)
    {
        var room = _rooms.GetRoom(code);
        if (room == null) return;
        var start = DateTime.UtcNow;

        for (int i = seconds; i >= 0; i--)
        {
            if (ct.IsCancellationRequested) return;
            await Clients.Group(code).SendAsync(HubEvents.TimerTick, i, ct);
            await Task.Delay(1000, CancellationToken.None);
            if (ct.IsCancellationRequested) return;
        }

        // Время вышло — подвести итог раунда
        await EvaluateRound(code);
    }

    // ── Ответ игрока ──────────────────────────────────────────────────────────

    public async Task SubmitAnswer(AnswerDto answer)
    {
        var room = _rooms.GetRoomByConnection(Context.ConnectionId);
        if (room == null || room.Status != RoomStatus.Playing) return;
        var player = room.Players.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
        if (player == null || player.LastAnswerIndex.HasValue) return;

        player.LastAnswerIndex = answer.AnswerIndex;
        player.AnswerTimeMs = answer.TimeMs;

        // Если все ответили — досрочно завершаем раунд
        if (room.Players.All(p => p.LastAnswerIndex.HasValue))
        {
            room.TimerCts?.Cancel();
            await Task.Delay(200);
            await EvaluateRound(room.Code);
        }
    }

    // ── Оценить раунд ────────────────────────────────────────────────────────

    private async Task EvaluateRound(string code)
    {
        var room = _rooms.GetRoom(code);
        if (room == null) return;

        var (_, correctIndex, explanation) = room.Questions[room.CurrentRound];
        var result = new RoundResultDto
        {
            CorrectIndex = correctIndex,
            Explanation = explanation,
            Scores = new(),
            Answers = new()
        };

        foreach (var p in room.Players)
        {
            bool correct = p.LastAnswerIndex == correctIndex;
            if (correct) p.Streak++;
            else p.Streak = 0;

            int earned = _score.Calculate(correct, p.AnswerTimeMs ?? room.TimerSeconds * 1000L, room.TimerSeconds, p.Streak);
            p.Score += earned;
            result.Scores[p.ConnectionId] = p.Score;
            result.Answers[p.ConnectionId] = correct;
        }

        await Clients.Group(code).SendAsync(HubEvents.RoundResult, result);
        room.CurrentRound++;

        await Task.Delay(3000); // показываем результат 3 секунды
        await SendNextQuestion(code);
    }

    // ── Финал игры ────────────────────────────────────────────────────────────

    private async Task FinishGame(string code)
    {
        var room = _rooms.GetRoom(code);
        if (room == null) return;
        room.Status = RoomStatus.Finished;

        var winner = room.Players.OrderByDescending(p => p.Score).FirstOrDefault();
        var result = new GameResultDto
        {
            WinnerId = winner?.ConnectionId,
            WinnerNickname = winner?.Nickname,
            FinalScores = room.Players.ToDictionary(p => p.ConnectionId, p => p.Score),
            XpEarned = 50
        };

        await Clients.Group(code).SendAsync(HubEvents.GameFinished, result);
    }

    // ── Выйти из комнаты ─────────────────────────────────────────────────────

    public async Task LeaveRoom()
    {
        var room = _rooms.GetRoomByConnection(Context.ConnectionId);
        if (room == null) return;
        var player = room.Players.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
        _rooms.RemovePlayer(Context.ConnectionId);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, room.Code);
        if (player != null)
            await Clients.Group(room.Code).SendAsync(HubEvents.PlayerLeft, player.Nickname);
    }

    private Task SendError(string msg) =>
        Clients.Caller.SendAsync(HubEvents.Error, msg);
}
