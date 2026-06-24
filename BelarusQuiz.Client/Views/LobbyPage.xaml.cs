using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using BelarusQuiz.Shared.Models;

namespace BelarusQuiz.Client.Views;

public partial class LobbyPage : Page
{
    private readonly AppState _app = AppState.Instance;
    private readonly bool _isHost;
    private bool _isReady;
    private RoomInfo _room;

    public LobbyPage(RoomInfo room, bool isHost)
    {
        InitializeComponent();
        _isHost = isHost;
        _room = room;
        UpdateUI(room);
        SubscribeEvents();
    }

    private void SubscribeEvents()
    {
        _app.SignalR.OnPlayerJoined      += r => Dispatcher.Invoke(() => UpdateUI(r));
        _app.SignalR.OnPlayerLeft        += _ => Dispatcher.Invoke(() => { /* handled via next join */ });
        _app.SignalR.OnPlayerReadyChanged+= r => Dispatcher.Invoke(() => UpdateUI(r));
        _app.SignalR.OnGameStarted       += r => Dispatcher.Invoke(() =>
            _app.Nav.Navigate(new GamePage(_app.Nickname)));
    }

    private void UpdateUI(RoomInfo room)
    {
        _room = room;
        TbCode.Text    = room.Code;
        TbRounds.Text  = $"📋 Раундов: {room.MaxRounds}";
        TbTimer.Text   = $"⏱ Таймер: {room.TimerSeconds} сек.";

        var items = room.Players.Select(p => new PlayerViewModel(p)).ToList();
        PlayersList.ItemsSource = items;

        bool allReady = room.Players.Count > 0 && room.Players.All(p => p.IsReady);
        BtnStart.IsEnabled = allReady && _isHost;
        TbWaiting.Visibility = room.Players.Count < 2 ? Visibility.Visible : Visibility.Collapsed;
    }

    private async void BtnReady_Click(object sender, RoutedEventArgs e)
    {
        _isReady = !_isReady;
        BtnReady.Content = _isReady ? "✖  НЕ ГОТОВ" : "✔  Я ГОТОВ";
        BtnReady.Style = _isReady
            ? (Style)FindResource("BtnDark")
            : (Style)FindResource("BtnGreen");
        await _app.SignalR.SetReady(_isReady);
    }

    private async void BtnStart_Click(object sender, RoutedEventArgs e)
    {
        // Хост помечает себя готовым для старта
        await _app.SignalR.SetReady(true);
    }

    private async void BtnLeave_Click(object sender, RoutedEventArgs e)
    {
        await _app.SignalR.LeaveRoom();
        _app.Nav.Navigate(new MainMenuPage());
    }
}

public class PlayerViewModel
{
    public string Nickname   { get; }
    public string StatusText { get; }
    public SolidColorBrush StatusColor { get; }

    public PlayerViewModel(PlayerInfo p)
    {
        Nickname    = p.Nickname;
        StatusText  = p.IsReady ? "Готов" : "Не готов";
        StatusColor = p.IsReady
            ? new SolidColorBrush(Color.FromRgb(0x2E, 0xCC, 0x40))
            : new SolidColorBrush(Color.FromRgb(0xE7, 0x4C, 0x3C));
    }
}
