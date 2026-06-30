using System.Net;
using System.Net.Sockets;
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
        ShowShareInfo(room);
        SubscribeEvents();
    }

    // ── Показываем IP и код для шаринга ──────────────────────────────────────
    private void ShowShareInfo(RoomInfo room)
    {
        // Получаем локальный IP
        string localIp = GetLocalIP();
        string serverUrl = _app.ServerUrl;

        // Если в настройках стоит localhost — заменяем на реальный IP
        if (serverUrl.Contains("localhost") || serverUrl.Contains("127.0.0.1"))
            serverUrl = serverUrl.Replace("localhost", localIp).Replace("127.0.0.1", localIp);

        TbShareInfo.Text = $"Сервер: {serverUrl}   Код: {room.Code}";
    }

    private static string GetLocalIP()
    {
        try
        {
            // Самый надёжный способ получить реальный IP
            using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0);
            socket.Connect("8.8.8.8", 65530);
            return (socket.LocalEndPoint as IPEndPoint)?.Address.ToString() ?? "localhost";
        }
        catch { return "localhost"; }
    }

    private void SubscribeEvents()
    {
        _app.SignalR.OnPlayerJoined += r => Dispatcher.Invoke(() => UpdateUI(r));
        _app.SignalR.OnPlayerLeft += _ => Dispatcher.Invoke(() => { });
        _app.SignalR.OnPlayerReadyChanged += r => Dispatcher.Invoke(() => UpdateUI(r));
        _app.SignalR.OnGameStarted += _ => Dispatcher.Invoke(() =>
            _app.Nav.Navigate(new GamePage(_app.Nickname)));
    }

    private void UpdateUI(RoomInfo room)
    {
        _room = room;
        TbCode.Text = room.Code;
        TbRounds.Text = $"Раундов: {room.MaxRounds}";
        TbTimer.Text = $"Таймер: {room.TimerSeconds} сек.";

        PlayersList.ItemsSource = room.Players.Select(p => new PlayerViewModel(p)).ToList();

        bool allReady = room.Players.Count > 0 && room.Players.All(p => p.IsReady);
        BtnStart.IsEnabled = allReady && _isHost;
        TbWaiting.Visibility = room.Players.Count < 2 ? Visibility.Visible : Visibility.Collapsed;
    }

    private async void BtnReady_Click(object sender, RoutedEventArgs e)
    {
        _isReady = !_isReady;
        BtnReady.Content = _isReady ? "✖ НЕ ГОТОВ" : "✔ Я ГОТОВ";
        BtnReady.Style = _isReady
            ? (Style)FindResource("BtnDark")
            : (Style)FindResource("BtnGreen");
        await _app.SignalR.SetReady(_isReady);
    }

    private async void BtnStart_Click(object sender, RoutedEventArgs e)
        => await _app.SignalR.SetReady(true);

    private async void BtnLeave_Click(object sender, RoutedEventArgs e)
    {
        await _app.SignalR.LeaveRoom();
        _app.Nav.Navigate(new MainMenuPage());
    }

    // Копировать данные подключения в буфер обмена
    private void BtnCopy_Click(object sender, RoutedEventArgs e)
    {
        Clipboard.SetText(TbShareInfo.Text);
        BtnCopy.Content = "Скопировано!";
    }
}

public class PlayerViewModel
{
    public string Nickname { get; }
    public string Initials { get; }   // ← добавлено для аватара в XAML
    public string StatusText { get; }
    public SolidColorBrush StatusColor { get; }

    public PlayerViewModel(PlayerInfo p)
    {
        Nickname = p.Nickname;
        Initials = p.Nickname.Length >= 2 ? p.Nickname[..2].ToUpper() : p.Nickname.ToUpper();
        StatusText = p.IsReady ? "Готов ✔" : "Не готов";
        StatusColor = p.IsReady
            ? new SolidColorBrush(Color.FromRgb(0x2E, 0xCC, 0x40))
            : new SolidColorBrush(Color.FromRgb(0xE7, 0x4C, 0x3C));
    }
}