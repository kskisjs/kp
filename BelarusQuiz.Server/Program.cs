

using BelarusQuiz.Server.Data;
using BelarusQuiz.Server.Hubs;
using BelarusQuiz.Server.Models;
using BelarusQuiz.Server.Services;
using BelarusQuiz.Shared.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=quiz.db"));

builder.Services.AddSignalR();
builder.Services.AddSingleton<RoomService>();
builder.Services.AddSingleton<ScoreService>();
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.UseCors();
app.MapHub<GameHub>("/gamehub");

// ── Хелпер: хэш пароля ───────────────────────────────────────────────────────
static string HashPassword(string password)
{
    using var sha = SHA256.Create();
    var bytes = Encoding.UTF8.GetBytes(password + "BelarusQuiz_Salt_2024");
    return Convert.ToHexString(sha.ComputeHash(bytes));
}

// ── POST /auth/register ───────────────────────────────────────────────────────
app.MapPost("/auth/register", async (RegisterDto dto, AppDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(dto.Login) || string.IsNullOrWhiteSpace(dto.Password))
        return Results.Ok(new LoginResultDto { Success = false, Error = "Заполните все поля" });

    if (dto.Login.Length < 3)
        return Results.Ok(new LoginResultDto { Success = false, Error = "Логин минимум 3 символа" });

    if (dto.Password.Length < 4)
        return Results.Ok(new LoginResultDto { Success = false, Error = "Пароль минимум 4 символа" });

    if (await db.Users.AnyAsync(u => u.Login == dto.Login))
        return Results.Ok(new LoginResultDto { Success = false, Error = "Логин уже занят" });

    var nickname = string.IsNullOrWhiteSpace(dto.Nickname) ? dto.Login : dto.Nickname.Trim();

    var user = new User
    {
        Login = dto.Login.Trim(),
        Nickname = nickname,
        PasswordHash = HashPassword(dto.Password)
    };

    db.Users.Add(user);
    await db.SaveChangesAsync();

    return Results.Ok(new LoginResultDto
    {
        Success = true,
        Nickname = user.Nickname,
        Wins = user.Wins,
        GamesPlayed = user.GamesPlayed,
        TotalScore = user.TotalScore
    });
});

// ── POST /auth/login ──────────────────────────────────────────────────────────
app.MapPost("/auth/login", async (LoginDto dto, AppDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(dto.Login) || string.IsNullOrWhiteSpace(dto.Password))
        return Results.Ok(new LoginResultDto { Success = false, Error = "Введите логин и пароль" });

    var user = await db.Users.FirstOrDefaultAsync(u => u.Login == dto.Login.Trim());
    if (user == null || user.PasswordHash != HashPassword(dto.Password))
        return Results.Ok(new LoginResultDto { Success = false, Error = "Неверный логин или пароль" });

    return Results.Ok(new LoginResultDto
    {
        Success = true,
        Nickname = user.Nickname,
        Wins = user.Wins,
        GamesPlayed = user.GamesPlayed,
        TotalScore = user.TotalScore
    });
});

// ── POST /game/stats — обновить статистику после игры ────────────────────────
app.MapPost("/game/stats", async (UpdateStatsDto dto, AppDbContext db) =>
{
    var user = await db.Users.FirstOrDefaultAsync(u => u.Login == dto.Login);
    if (user == null) return Results.NotFound();

    user.GamesPlayed++;
    if (dto.Won) user.Wins++;
    user.TotalScore += dto.Score;
    await db.SaveChangesAsync();
    return Results.Ok();
});

// ── GET /leaderboard — таблица лидеров ───────────────────────────────────────
app.MapGet("/leaderboard", async (AppDbContext db) =>
{
    var users = await db.Users
        .OrderByDescending(u => u.TotalScore)
        .Take(20)
        .ToListAsync();

    var entries = users.Select((u, i) => new LeaderboardEntry
    {
        Rank = i + 1,
        Nickname = u.Nickname,
        Level = Math.Max(1, u.GamesPlayed / 5 + 1),
        TotalScore = u.TotalScore
    }).ToList();

    return Results.Ok(entries);
});

app.MapGet("/", () => "BelarusQuiz Server running!");
Console.WriteLine("=== Сервер запущен на http://localhost:5000 ===");
app.Run("http://0.0.0.0:5000");

// ── Внутренняя модель ─────────────────────────────────────────────────────────
record UpdateStatsDto(string Login, bool Won, int Score);