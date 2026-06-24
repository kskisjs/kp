// Путь: BelarusQuiz.Client/Views/MainMenuPage.xaml.cs  (ЗАМЕНИТЬ)

using System.Windows;
using System.Windows.Controls;
using BelarusQuiz.Shared.Enums;

namespace BelarusQuiz.Client.Views;

public partial class MainMenuPage : Page
{
    private readonly AppState _app = AppState.Instance;
    private bool _handlersAttached = false;

    public MainMenuPage()
    {
        InitializeComponent();
        UpdatePlayerInfo();
    }

    private void UpdatePlayerInfo()
    {
        if (!_app.IsLoggedIn) return;

        TbNickname.Text = _app.Nickname;
        TbLevel.Text = $"Уровень {_app.Level}";
        TbLevelBig.Text = $"Ур. {_app.Level}";
        TbInitials.Text = _app.Nickname.Length >= 2
            ? _app.Nickname[..2].ToUpper()
            : _app.Nickname.ToUpper();

        TbWins.Text = _app.UserWins.ToString();
        TbGames.Text = _app.UserGamesPlayed.ToString();
        TbTotalScore.Text = _app.UserTotalScore.ToString();
        // PbXP убран из XAML — строку ниже не нужно
    }

    private async void BtnQuick_Click(object sender, RoutedEventArgs e)
    {
        if (!ValidateLogin()) return;
        SetStatus("Подключение...");
        try
        {
            await EnsureConnected();
            AttachHandlers();
            await _app.SignalR.CreateRoom(_app.Nickname, 10, 15, QuizCategory.All);
        }
        catch (Exception ex) { SetStatus("❌ " + ex.Message); }
    }

    private async void BtnCreate_Click(object sender, RoutedEventArgs e)
    {
        if (!ValidateLogin()) return;
        SetStatus("Подключение...");
        try
        {
            await EnsureConnected();
            AttachHandlers();
            await _app.SignalR.CreateRoom(_app.Nickname, 10, 15, QuizCategory.All);
        }
        catch (Exception ex) { SetStatus("❌ " + ex.Message); }
    }

    private async void BtnJoin_Click(object sender, RoutedEventArgs e)
    {
        if (!ValidateLogin()) return;
        try
        {
            await EnsureConnected();
            _app.Nav.Navigate(new JoinRoomPage());
        }
        catch (Exception ex) { SetStatus("❌ " + ex.Message); }
    }

    private void BtnSolo_Click(object sender, RoutedEventArgs e)
        => _app.Nav.Navigate(new SoloGamePage());

    private void BtnSettings_Click(object sender, RoutedEventArgs e)
        => _app.Nav.Navigate(new SettingsPage());

    private void BtnLeaderboard_Click(object sender, RoutedEventArgs e)
        => _app.Nav.Navigate(new LeaderboardPage());

    private void BtnInfo_Click(object sender, RoutedEventArgs e)
        => MessageBox.Show(
            "🌿 Беларусь: Знай свой край\n\n" +
            "Многопользовательская викторина-баттл\n" +
            "о географии, истории, культуре и природе Беларуси.\n\n" +
            "60 вопросов в 6 категориях.\n" +
            "C# · WPF · SignalR · .NET 8",
            "О игре", MessageBoxButton.OK, MessageBoxImage.Information);

    private async Task EnsureConnected()
    {
        if (!_app.SignalR.IsConnected)
            await _app.SignalR.ConnectAsync(_app.ServerUrl);
    }

    private void AttachHandlers()
    {
        if (_handlersAttached) return;
        _handlersAttached = true;

        _app.SignalR.OnRoomCreated += room =>
            Dispatcher.Invoke(() => _app.Nav.Navigate(new LobbyPage(room, isHost: true)));
        _app.SignalR.OnError += err =>
            Dispatcher.Invoke(() => SetStatus("❌ " + err));
    }

    private bool ValidateLogin()
    {
        if (!_app.IsLoggedIn)
        {
            _app.Nav.Navigate(new LoginPage());
            return false;
        }
        return true;
    }

    private void SetStatus(string msg) => TbStatus.Text = msg;
}