using BelarusQuiz.Shared.Enums;

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
