// Путь: BelarusQuiz.Client/Views/LeaderboardPage.xaml.cs  (НОВЫЙ ФАЙЛ)

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using BelarusQuiz.Shared.Models;

namespace BelarusQuiz.Client.Views;

public partial class LeaderboardPage : Page
{
    private readonly AppState _app = AppState.Instance;
    private List<LeaderboardEntry> _entries = new();

    public LeaderboardPage()
    {
        InitializeComponent();
        Loaded += async (_, _) => await LoadLeaderboard();
    }

    private async Task LoadLeaderboard()
    {
        TbLoading.Visibility = Visibility.Visible;
        _entries = await _app.Http.GetLeaderboardAsync();
        TbLoading.Visibility = Visibility.Collapsed;

        var vms = _entries.Select(e => new LeaderboardViewModel(e)).ToList();
        LeaderList.ItemsSource = vms;

        // Моё место
        var mine = _entries.FirstOrDefault(e => e.Nickname == _app.Nickname);
        TbMyRank.Text = mine != null ? $"{mine.Rank}" : "—";
        TbMyScore.Text = mine != null ? $"{mine.TotalScore} очков" : "";
    }

    private void BtnBack_Click(object sender, RoutedEventArgs e)
        => _app.Nav.Navigate(new MainMenuPage());

    private void BtnTabGlobal_Click(object sender, RoutedEventArgs e)
    {
        // Уже на глобальном
        BtnTabGlobal.Style = (Style)FindResource("BtnTabActive");
        BtnTabFriends.Style = (Style)FindResource("BtnTab");
        LeaderList.ItemsSource = _entries.Select(e => new LeaderboardViewModel(e)).ToList();
    }

    private void BtnTabFriends_Click(object sender, RoutedEventArgs e)
    {
        BtnTabFriends.Style = (Style)FindResource("BtnTabActive");
        BtnTabGlobal.Style = (Style)FindResource("BtnTab");
        // Пока показываем только себя (система друзей — будущее расширение)
        var mine = _entries.Where(e => e.Nickname == _app.Nickname)
                           .Select(e => new LeaderboardViewModel(e)).ToList();
        LeaderList.ItemsSource = mine;
    }
}

// ── ViewModel для строки таблицы ─────────────────────────────────────────────

public class LeaderboardViewModel
{
    public int Rank { get; }
    public string Nickname { get; }
    public int Level { get; }
    public int TotalScore { get; }

    // Отображение места: 🥇🥈🥉 или число
    public string RankDisplay => Rank switch
    {
        1 => "🥇",
        2 => "🥈",
        3 => "🥉",
        _ => Rank.ToString()
    };

    public string RankColor => Rank switch
    {
        1 => "#F1C40F",
        2 => "#BDC3C7",
        3 => "#CD7F32",
        _ => "#7CB87E"
    };

    // Первые две буквы никнейма для аватара
    public string Initials => Nickname.Length >= 2
        ? Nickname[..2].ToUpper()
        : Nickname.ToUpper();

    // Цвет аватара по индексу
    public string AvatarColor
    {
        get
        {
            string[] colors = { "#1A6E30", "#C0392B", "#2980B9", "#8E44AD", "#D35400", "#16A085" };
            return colors[Math.Abs(Nickname.GetHashCode()) % colors.Length];
        }
    }

    public LeaderboardViewModel(LeaderboardEntry e)
    {
        Rank = e.Rank;
        Nickname = e.Nickname;
        Level = e.Level;
        TotalScore = e.TotalScore;
    }
}