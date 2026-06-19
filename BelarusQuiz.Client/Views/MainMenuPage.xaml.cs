using System.Windows;
using System.Windows.Controls;

namespace BelarusQuiz.Client.Views;

public partial class MainMenuPage : Page
{
    private readonly AppState _app = AppState.Instance;
    private bool _handlersAttached = false;

    public MainMenuPage() => InitializeComponent();

    private async void BtnCreate_Click(object sender, RoutedEventArgs e)
    {
        if (!Validate()) return;
        SetStatus("Подключение к серверу...");
        try
        {
            _app.ServerUrl = TbServer.Text.Trim();
            _app.Nickname = TbNickname.Text.Trim();
            await _app.SignalR.ConnectAsync(_app.ServerUrl);

            if (!_handlersAttached)
            {
                _handlersAttached = true;
                _app.SignalR.OnRoomCreated += room =>
                    Dispatcher.Invoke(() => _app.Nav.Navigate(new LobbyPage(room, isHost: true)));
                _app.SignalR.OnError += err =>
                    Dispatcher.Invoke(() => SetStatus("❌ " + err));
            }

            await _app.SignalR.CreateRoom(_app.Nickname);
        }
        catch (Exception ex)
        {
            SetStatus("❌ Не удалось подключиться: " + ex.Message);
        }
    }

    private void BtnJoin_Click(object sender, RoutedEventArgs e)
    {
        if (!Validate()) return;
        _app.ServerUrl = TbServer.Text.Trim();
        _app.Nickname = TbNickname.Text.Trim();
        _app.Nav.Navigate(new JoinRoomPage());
    }

    private void BtnSolo_Click(object sender, RoutedEventArgs e)
    {
        _app.Nickname = string.IsNullOrWhiteSpace(TbNickname.Text) ? "Игрок" : TbNickname.Text.Trim();
        _app.Nav.Navigate(new SoloGamePage());
    }

    private bool Validate()
    {
        if (string.IsNullOrWhiteSpace(TbNickname.Text))
        {
            SetStatus("⚠ Введите никнейм"); return false;
        }
        if (string.IsNullOrWhiteSpace(TbServer.Text))
        {
            SetStatus("⚠ Введите адрес сервера"); return false;
        }
        return true;
    }

    private void SetStatus(string msg) => TbStatus.Text = msg;
}
