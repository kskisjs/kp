using BelarusQuiz.Shared.Enums;

// Путь: BelarusQuiz.Shared/Models/Models.cs  (ПОЛНАЯ ЗАМЕНА)

namespace BelarusQuiz.Shared.Models;

public class PlayerInfo
{
    public string Id { get; set; } = "";
    public string Nickname { get; set; } = "";
    public int Score { get; set; }
    public bool IsReady { get; set; }
    public int Streak { get; set; }
}

public class RoomInfo
{
    public string Code { get; set; } = "";
    public GameMode Mode { get; set; }
    public QuizCategory Category { get; set; } = QuizCategory.All;
    public int MaxRounds { get; set; } = 10;
    public int TimerSeconds { get; set; } = 15;
    public RoomStatus Status { get; set; }
    public List<PlayerInfo> Players { get; set; } = new();
}

public class QuestionDto
{
    public int Id { get; set; }
    public string Text { get; set; } = "";
    public string Category { get; set; } = "";
    public QuizCategory CategoryEnum { get; set; } = QuizCategory.All;
    public List<string> Options { get; set; } = new();
    public int RoundNumber { get; set; }
    public int TotalRounds { get; set; }
}

public class AnswerDto
{
    public int QuestionId { get; set; }
    public int AnswerIndex { get; set; }
    public long TimeMs { get; set; }
}

public class RoundResultDto
{
    public int CorrectIndex { get; set; }
    public Dictionary<string, int> Scores { get; set; } = new();
    public Dictionary<string, bool> Answers { get; set; } = new();
    public string? Explanation { get; set; }
}

public class GameResultDto
{
    public string? WinnerId { get; set; }
    public string? WinnerNickname { get; set; }
    public Dictionary<string, int> FinalScores { get; set; } = new();
    public int XpEarned { get; set; }
}

public class LeaderboardEntry
{
    public int Rank { get; set; }
    public string Nickname { get; set; } = "";
    public int Level { get; set; }
    public int TotalScore { get; set; }
}

// ── Авторизация ──────────────────────────────────────────────────────────────

public class RegisterDto
{
    public string Login { get; set; } = "";
    public string Nickname { get; set; } = "";   // имя отображения
    public string Password { get; set; } = "";
}

public class LoginDto
{
    public string Login { get; set; } = "";
    public string Password { get; set; } = "";
}

public class LoginResultDto
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public string? Nickname { get; set; }
    public int Wins { get; set; }
    public int GamesPlayed { get; set; }
    public int TotalScore { get; set; }           // ДОБАВЛЕНО
}

// ── SignalR события ──────────────────────────────────────────────────────────

public static class HubEvents
{
    public const string PlayerJoined = "PlayerJoined";
    public const string PlayerLeft = "PlayerLeft";
    public const string PlayerReadyChanged = "PlayerReadyChanged";
    public const string GameStarted = "GameStarted";
    public const string QuestionReceived = "QuestionReceived";
    public const string TimerTick = "TimerTick";
    public const string RoundResult = "RoundResult";
    public const string GameFinished = "GameFinished";
    public const string Error = "Error";
}