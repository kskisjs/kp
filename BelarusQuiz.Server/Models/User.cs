namespace BelarusQuiz.Server.Models;

public class User
{
    public int Id { get; set; }

    public string Login { get; set; } = "";

    public string PasswordHash { get; set; } = "";

    public int Wins { get; set; }

    public int GamesPlayed { get; set; }
}