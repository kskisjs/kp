using BelarusQuiz.Server.Data;
using BelarusQuiz.Server.Hubs;
using BelarusQuiz.Server.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlite("Data Source=quiz.db");
});

builder.Services.AddSignalR();
builder.Services.AddSingleton<RoomService>();
builder.Services.AddSingleton<ScoreService>();

builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.AllowAnyOrigin()
     .AllowAnyHeader()
     .AllowAnyMethod()));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.UseCors();

app.MapHub<GameHub>("/gamehub");

app.MapGet("/", () => "BelarusQuiz Server running!");

Console.WriteLine("=== Сервер запущен на http://localhost:5000 ===");

app.Run("http://0.0.0.0:5000");