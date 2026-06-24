// Путь: BelarusQuiz.Client/Services/HttpService.cs  (НОВЫЙ ФАЙЛ)

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

    public async Task UpdateStatsAsync(string login, bool won, int score)
    {
        try
        {
            await _http.PostAsJsonAsync($"{BaseUrl}/game/stats",
                new { Login = login, Won = won, Score = score });
        }
        catch { /* игнорируем — статистика не критична */ }
    }

    public async Task<List<LeaderboardEntry>> GetLeaderboardAsync()
    {
        try
        {
            var result = await _http.GetFromJsonAsync<List<LeaderboardEntry>>($"{BaseUrl}/leaderboard");
            return result ?? new();
        }
        catch
        {
            return new();
        }
    }
}