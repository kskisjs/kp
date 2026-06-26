using System;
namespace BelarusQuiz.Server.Models;

public class UserAchievement
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string AchievementName { get; set; }
    public DateTime UnlockedDate { get; set; }

    public User User { get; set; }
}