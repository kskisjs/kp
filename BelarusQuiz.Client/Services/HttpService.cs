using System.Net.Http;
using System.Net.Http.Json;
using BelarusQuiz.Shared.Models;

namespace BelarusQuiz.Client.Services;

public class HttpService
{
    private readonly HttpClient _http = new();
    private string BaseUrl => AppState.Instance.ServerUrl;

    public async Task<LoginResultDto> RegisterAsync(RegisterDto dto)
    {
        try
        {
            var resp = await _http.PostAsJsonAsync($"{BaseUrl}/auth/register", dto);
            return await resp.Content.ReadFromJsonAsync<LoginResultDto>()
                   ?? new LoginResultDto { Success = false, Error = "Ошибка сервера" };
        }
        catch (Exception ex)
        {
            return new LoginResultDto { Success = false, Error = "Нет связи с сервером: " + ex.Message };
        }
    }

    public async Task<LoginResultDto> LoginAsync(LoginDto dto)
    {
        try
        {
            var resp = await _http.PostAsJsonAsync($"{BaseUrl}/auth/login", dto);
            return await resp.Content.ReadFromJsonAsync<LoginResultDto>()
                   ?? new LoginResultDto { Success = false, Error = "Ошибка сервера" };
        }
        catch (Exception ex)
        {
            return new LoginResultDto { Success = false, Error = "Нет связи с сервером: " + ex.Message };
        }
    }

    // Старый метод (оставлен для совместимости)
    public async Task UpdateStatsAsync(string login, bool won, int score)
    {
        await SaveGameAsync(login, won, score, 0, "Соперник", "Мультиплеер", 10);
    }

    // ★ НОВЫЙ метод — полная статистика игры
    public async Task SaveGameAsync(
        string login,
        bool won,
        int myScore,
        int opponentScore = 0,
        string opponentNickname = "Бот",
        string category = "Все категории",
        int rounds = 10)
    {
        try
        {
            await _http.PostAsJsonAsync($"{BaseUrl}/game/stats", new
            {
                Login = login,
                Won = won,
                Score = myScore,
                OpponentScore = opponentScore,
                OpponentNickname = opponentNickname,
                Category = category,
                Rounds = rounds
            });
        }
        catch { /* статистика не критична */ }
    }

    public async Task<List<LeaderboardEntry>> GetLeaderboardAsync()
    {
        try
        {
            return await _http.GetFromJsonAsync<List<LeaderboardEntry>>($"{BaseUrl}/leaderboard")
                   ?? new();
        }
        catch { return new(); }
    }
}