// Путь: BelarusQuiz.Client/AppState.cs  (ПОЛНАЯ ЗАМЕНА)

using BelarusQuiz.Client.Services;

namespace BelarusQuiz.Client;

public class AppState
{
    public static AppState Instance { get; } = new();

    public SignalRService SignalR { get; } = new();
    public NavigationService Nav { get; } = new();
    public HttpService Http { get; } = new();

    // ── Соединение ────────────────────────────────────────────────────────────
    public string ServerUrl { get; set; } = "http://localhost:5000";
    public string MyConnectionId { get; set; } = "";

    // ── Пользователь (после входа) ────────────────────────────────────────────
    public string UserLogin { get; set; } = "";
    public string Nickname { get; set; } = "";
    public int UserWins { get; set; }
    public int UserGamesPlayed { get; set; }
    public int UserTotalScore { get; set; }

    // ── Уровень (рассчитывается) ───────────────────────────────────────────────
    public int Level => Math.Max(1, UserGamesPlayed / 5 + 1);
    public bool IsLoggedIn => !string.IsNullOrEmpty(UserLogin);
}