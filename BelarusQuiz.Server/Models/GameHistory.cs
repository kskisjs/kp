using System;
namespace BelarusQuiz.Server.Models;

public class GameHistory
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int Score { get; set; }
    public DateTime DatePlayed { get; set; }
    public bool IsWin { get; set; }

    public User User { get; set; }
}