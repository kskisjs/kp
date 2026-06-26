
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using BelarusQuiz.Shared.Models;

namespace BelarusQuiz.Client.Views;

public partial class ResultsPage : Page
{
    private readonly AppState _app = AppState.Instance;
    private readonly GameResultDto _result;
    private readonly string _myNickname;

    public ResultsPage(GameResultDto result, string myNickname)
    {
        InitializeComponent();
        _result = result;
        _myNickname = myNickname;
        BuildUI();
        SaveStatsIfMultiplayer(); // Сохраняем статистику для мультиплеера
    }

    private void BuildUI()
    {
        bool iWon = _result.WinnerNickname == _myNickname;

        TbTrophy.Text = iWon ? "🏆" : "💀";
        TbResult.Text = iWon ? "Победа!" : "Поражение";
        TbResult.Foreground = iWon
            ? new SolidColorBrush(Color.FromRgb(0x2E, 0xCC, 0x40))
            : new SolidColorBrush(Color.FromRgb(0xE7, 0x4C, 0x3C));
        TbSubtitle.Text = iWon ? "Отличная игра!" : "В следующий раз повезёт!";
        TbXp.Text = $"+{_result.XpEarned}";

        var medals = new[] { "🥇", "🥈", "🥉", "4️⃣" };
        var sorted = _result.FinalScores
            .OrderByDescending(kv => kv.Value)
            .Select((kv, i) =>
            {
                bool isMe = kv.Key == _myNickname;
                return new ScoreRow
                {
                    Medal = i < medals.Length ? medals[i] : "•",
                    Nickname = kv.Key,
                    // ★ ИСПРАВЛЕНИЕ: очки крупно и с единицей + яркий цвет
                    Score = $"{kv.Value:N0} очков",
                    ScoreColor = isMe
                        ? new SolidColorBrush(Color.FromRgb(0xFF, 0xD7, 0x00))   // Золотой для своих
                        : new SolidColorBrush(Color.FromRgb(0x7A, 0xAA, 0x7C)),  // Зелёный для чужих
                    RowBackground = isMe
                        ? new SolidColorBrush(Color.FromRgb(0x1A, 0x3A, 0x1E))
                        : new SolidColorBrush(Color.FromRgb(0x0E, 0x22, 0x12)),
                    IsMe = isMe
                };
            }).ToList();

        ScoresList.ItemsSource = sorted;
    }

    // Для мультиплеера — статистика (соло сохраняется прямо в SoloGamePage)
    private async void SaveStatsIfMultiplayer()
    {
        if (!_app.IsLoggedIn) return;

        // Проверяем: это мультиплеер (нет бота в именах)
        bool isSolo = _result.FinalScores.Keys.Any(k => k.Contains("Бот"));
        if (isSolo) return; // Соло уже сохраняется в SoloGamePage.EndGame()

        bool iWon = _result.WinnerNickname == _myNickname;
        int myScore = _result.FinalScores.TryGetValue(_myNickname, out var ms) ? ms : 0;
        int oppScore = _result.FinalScores.Where(kv => kv.Key != _myNickname)
                                          .Select(kv => kv.Value).FirstOrDefault();
        string oppNickname = _result.FinalScores.Keys.FirstOrDefault(k => k != _myNickname) ?? "Соперник";

        await _app.Http.SaveGameAsync(
            login: _app.UserLogin,
            won: iWon,
            myScore: myScore,
            opponentScore: oppScore,
            opponentNickname: oppNickname,
            category: "Мультиплеер",
            rounds: 10
        );

        _app.UserGamesPlayed++;
        if (iWon) _app.UserWins++;
        _app.UserTotalScore += myScore;
    }

    private void BtnMenu_Click(object sender, RoutedEventArgs e)
        => _app.Nav.Navigate(new MainMenuPage());

    private async void BtnPlayAgain_Click(object sender, RoutedEventArgs e)
    {
        await _app.SignalR.LeaveRoom();
        _app.Nav.Navigate(new MainMenuPage());
    }
}

public class ScoreRow
{
    public string Medal { get; set; } = "";
    public string Nickname { get; set; } = "";
    public string Score { get; set; } = "";
    public SolidColorBrush ScoreColor { get; set; } = new(Colors.White);
    public SolidColorBrush RowBackground { get; set; } = new(Color.FromRgb(0x0E, 0x22, 0x12));
    public bool IsMe { get; set; }
}