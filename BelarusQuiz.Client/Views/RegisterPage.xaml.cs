// Путь: BelarusQuiz.Client/Views/RegisterPage.xaml.cs  (НОВЫЙ ФАЙЛ)

using System.Windows;
using System.Windows.Controls;
using BelarusQuiz.Shared.Models;

namespace BelarusQuiz.Client.Views;

public partial class RegisterPage : Page
{
    private readonly AppState _app = AppState.Instance;

    public RegisterPage() => InitializeComponent();

    private async void BtnRegister_Click(object sender, RoutedEventArgs e)
    {
        var nickname = TbNickname.Text.Trim();
        var login = TbLogin.Text.Trim();
        var password = PbPassword.Password;
        var confirm = PbConfirm.Password;

        if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
        {
            SetStatus("⚠ Заполните все поля");
            return;
        }
        if (password != confirm)
        {
            SetStatus("⚠ Пароли не совпадают");
            return;
        }
        if (password.Length < 4)
        {
            SetStatus("⚠ Пароль минимум 4 символа");
            return;
        }

        SetStatus("Регистрация...");

        var result = await _app.Http.RegisterAsync(new RegisterDto
        {
            Login = login,
            Nickname = string.IsNullOrEmpty(nickname) ? login : nickname,
            Password = password
        });

        if (!result.Success)
        {
            SetStatus("❌ " + result.Error);
            return;
        }

        // Автовход после регистрации
        _app.UserLogin = login;
        _app.Nickname = result.Nickname ?? login;
        _app.UserWins = result.Wins;
        _app.UserGamesPlayed = result.GamesPlayed;
        _app.UserTotalScore = result.TotalScore;

        _app.Nav.Navigate(new MainMenuPage());
    }

    private void BtnBack_Click(object sender, RoutedEventArgs e)
        => _app.Nav.Navigate(new LoginPage());

    private void SetStatus(string msg) => TbStatus.Text = msg;
}