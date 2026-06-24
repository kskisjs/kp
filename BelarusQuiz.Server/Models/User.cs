namespace BelarusQuiz.Server.Models;

// Путь: BelarusQuiz.Server/Models/User.cs

public class User
{
    public int Id { get; set; }
    public string Login { get; set; } = "";
    public string Nickname { get; set; } = "";         // ДОБАВЛЕНО
    public string PasswordHash { get; set; } = "";
    public int Wins { get; set; }
    public int GamesPlayed { get; set; }
    public int TotalScore { get; set; }                // ДОБАВЛЕНО
}