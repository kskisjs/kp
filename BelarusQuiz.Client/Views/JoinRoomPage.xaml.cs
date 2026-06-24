using System.Windows;
using System.Windows.Controls;

namespace BelarusQuiz.Client.Views;

public partial class JoinRoomPage : Page
{
    private readonly AppState _app = AppState.Instance;
    private bool _handlersAttached = false;

    public JoinRoomPage() => InitializeComponent();

    private async void BtnConnect_Click(object sender, RoutedEventArgs e)
    {
        var code = TbCode.Text.Trim().ToUpper();
        if (code.Length < 4) { TbStatus.Text = "⚠ Введите код комнаты"; return; }

        if (string.IsNullOrWhiteSpace(_app.Nickname))
        {
            TbStatus.Text = "⚠ Вернитесь в меню и введите никнейм";
            return;
        }

        TbStatus.Text = "Подключение...";
        try
        {
            if (!_app.SignalR.IsConnected)
                await _app.SignalR.ConnectAsync(_app.ServerUrl);

            if (!_handlersAttached)
            {
                _handlersAttached = true;
                _app.SignalR.OnPlayerJoined += room =>
                    Dispatcher.Invoke(() => _app.Nav.Navigate(new LobbyPage(room, isHost: false)));
                _app.SignalR.OnError += err =>
                    Dispatcher.Invoke(() => TbStatus.Text = "❌ " + err);
            }

            await _app.SignalR.JoinRoom(code, _app.Nickname);
        }
        catch (Exception ex)
        {
            TbStatus.Text = "❌ " + ex.Message;
        }
    }

    private void BtnBack_Click(object sender, RoutedEventArgs e)
        => _app.Nav.Navigate(new MainMenuPage());
}
