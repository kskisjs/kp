// Путь: BelarusQuiz.Client/Views/LoginPage.xaml.cs  (НОВЫЙ ФАЙЛ)

using System.Windows;
using System.Windows.Controls;
using BelarusQuiz.Shared.Models;

namespace BelarusQuiz.Client.Views;

public partial class LoginPage : Page
{
    private readonly AppState _app = AppState.Instance;

    public LoginPage() => InitializeComponent();

    private async void BtnLogin_Click(object sender, RoutedEventArgs e)
    {
        var login = TbLogin.Text.Trim();
        var password = PbPassword.Password;
        var server = TbServer.Text.Trim();

        if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
        {
            SetStatus("⚠ Введите логин и пароль");
            return;
        }

        _app.ServerUrl = server;
        SetStatus("Подключение...");

        var result = await _app.Http.LoginAsync(new LoginDto
        {
            Login = login,
            Password = password
        });

        if (!result.Success)
        {
            SetStatus("❌ " + result.Error);
            return;
        }

        // Сохраняем данные пользователя
        _app.UserLogin = login;
        _app.Nickname = result.Nickname ?? login;
        _app.UserWins = result.Wins;
        _app.UserGamesPlayed = result.GamesPlayed;
        _app.UserTotalScore = result.TotalScore;

        _app.Nav.Navigate(new MainMenuPage());
    }

    private void BtnRegister_Click(object sender, RoutedEventArgs e)
    {
        _app.ServerUrl = TbServer.Text.Trim();
        _app.Nav.Navigate(new RegisterPage());
    }

    private void SetStatus(string msg) => TbStatus.Text = msg;
}