using BelarusQuiz.Server.Models;
using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<GameHistory> GameHistories => Set<GameHistory>();
    public DbSet<UserAchievement> UserAchievements => Set<UserAchievement>();
}