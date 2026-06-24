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
    }

    private void BuildUI()
    {
        bool iWon = _result.WinnerNickname == _myNickname;

        TbTrophy.Text    = iWon ? "🏆" : "🥈";
        TbResult.Text    = iWon ? "Победа!" : "Поражение";
        TbResult.Foreground = iWon
            ? new SolidColorBrush(Color.FromRgb(0x2E, 0xCC, 0x40))
            : new SolidColorBrush(Color.FromRgb(0xE7, 0x4C, 0x3C));

        TbXp.Text = $"+{_result.XpEarned}";

        var medals = new[] { "🥇", "🥈", "🥉", "4️⃣" };
        var sorted = _result.FinalScores
            .OrderByDescending(kv => kv.Value)
            .Select((kv, i) => new ScoreRow
            {
                Medal      = i < medals.Length ? medals[i] : "•",
                Nickname   = kv.Key, // В реальном проекте здесь никнейм
                Score      = $"{kv.Value} очков",
                ScoreColor = i == 0
                    ? new SolidColorBrush(Color.FromRgb(0xFF, 0xD7, 0x00))
                    : new SolidColorBrush(Color.FromRgb(0x7A, 0xAB, 0x7C))
            }).ToList();

        ScoresList.ItemsSource = sorted;
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
    public string Medal      { get; set; } = "";
    public string Nickname   { get; set; } = "";
    public string Score      { get; set; } = "";
    public SolidColorBrush ScoreColor { get; set; } = new(Colors.White);
}
