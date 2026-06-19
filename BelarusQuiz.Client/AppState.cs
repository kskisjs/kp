using BelarusQuiz.Client.Services;

namespace BelarusQuiz.Client;

/// <summary>Глобальное состояние приложения — один экземпляр на всё время работы.</summary>
public class AppState
{
    public static AppState Instance { get; } = new();

    public SignalRService  SignalR   { get; } = new();
    public NavigationService Nav     { get; } = new();

    public string Nickname  { get; set; } = "";
    public string ServerUrl { get; set; } = "http://localhost:5000";
    public string MyConnectionId { get; set; } = "";
}
