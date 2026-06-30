using System.Windows;
using System.Windows.Controls;

namespace BelarusQuiz.Client.Views;

public partial class SettingsPage : Page
{
    private readonly AppState _app = AppState.Instance;

    public SettingsPage()
    {
        InitializeComponent();
        LoadSettings();
    }

    private void LoadSettings()
    {
        TbNickname.Text = _app.Nickname;
        TbInitials.Text = _app.Nickname.Length >= 2
            ? _app.Nickname[..2].ToUpper()
            : _app.Nickname.ToUpper();

        // TbStats заменён на два отдельных поля в новом XAML:
        TbLoginText.Text = $"@{_app.UserLogin}";
        TbLevel.Text = $"Уровень {_app.Level}";

        TbWins.Text = _app.UserWins.ToString();
        TbGames.Text = _app.UserGamesPlayed.ToString();
        TbScore.Text = _app.UserTotalScore.ToString();
        TbServer.Text = _app.ServerUrl;
    }

    private void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        _app.ServerUrl = TbServer.Text.Trim();
        TbStatus.Text = "✔ Сохранено";
    }

    private async void BtnLogout_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show(
            "Выйти из аккаунта?", "Выход",
            MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes) return;

        _app.UserLogin = "";
        _app.Nickname = "";
        _app.UserWins = 0;
        _app.UserGamesPlayed = 0;
        _app.UserTotalScore = 0;

        await _app.SignalR.DisconnectAsync();
        _app.Nav.Navigate(new LoginPage());
    }

    private void BtnBack_Click(object sender, RoutedEventArgs e)
        => _app.Nav.Navigate(new MainMenuPage());
}