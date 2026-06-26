using System.Windows;
using System.Windows.Controls;
using BelarusQuiz.Shared.Enums;

namespace BelarusQuiz.Client.Views;

public partial class CreateRoomPage : Page
{
    private readonly AppState _app = AppState.Instance;

    // Текущий выбор
    private QuizCategory _selectedCategory = QuizCategory.All;
    private int _selectedRounds = 5;
    private int _selectedTimer = 15;

    // Словари кнопок для переключения стилей
    private Dictionary<QuizCategory, Button> _catButtons = new();
    private Dictionary<int, Button> _roundButtons = new();
    private Dictionary<int, Button> _timerButtons = new();

    private bool _handlersAttached = false;

    public CreateRoomPage()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _catButtons = new()
        {
            { QuizCategory.All,       BtnCatAll  },
            { QuizCategory.Geography, BtnCatGeo  },
            { QuizCategory.History,   BtnCatHist },
            { QuizCategory.Culture,   BtnCatCult },
            { QuizCategory.Nature,    BtnCatNat  },
            { QuizCategory.Symbols,   BtnCatSym  },
            { QuizCategory.People,    BtnCatPpl  },
        };
        _roundButtons = new() { { 5, BtnR5 }, { 10, BtnR10 }, { 15, BtnR15 }, { 20, BtnR20 } };
        _timerButtons = new() { { 10, BtnT10 }, { 15, BtnT15 }, { 30, BtnT30 } };
    }

    // ── Выбор категории ───────────────────────────────────────────────────────
    private void BtnCat_Click(object sender, RoutedEventArgs e)
    {
        var btn = (Button)sender;
        var tag = btn.Tag?.ToString() ?? "All";

        _selectedCategory = tag switch
        {
            "Geography" => QuizCategory.Geography,
            "History" => QuizCategory.History,
            "Culture" => QuizCategory.Culture,
            "Nature" => QuizCategory.Nature,
            "Symbols" => QuizCategory.Symbols,
            "People" => QuizCategory.People,
            _ => QuizCategory.All
        };

        // Обновляем стили кнопок
        var activeStyle = (Style)FindResource("BtnCatActive");
        var normalStyle = (Style)FindResource("BtnCat");
        foreach (var (_, b) in _catButtons)
            b.Style = normalStyle;
        btn.Style = activeStyle;

        TbSelectedCat.Text = tag switch
        {
            "Geography" => "🗺️ География",
            "History" => "📜 История",
            "Culture" => "🎭 Культура",
            "Nature" => "🌿 Природа",
            "Symbols" => "🏛️ Символика",
            "People" => "🧑 Знаменитые люди",
            _ => "🎲 Все категории"
        };
    }

    // ── Выбор раундов ─────────────────────────────────────────────────────────
    private void BtnRounds_Click(object sender, RoutedEventArgs e)
    {
        var btn = (Button)sender;
        _selectedRounds = int.Parse(btn.Tag.ToString()!);

        var activeStyle = (Style)FindResource("BtnCatActive");
        var normalStyle = (Style)FindResource("BtnCat");
        foreach (var (_, b) in _roundButtons) b.Style = normalStyle;
        btn.Style = activeStyle;

        TbSelectedRounds.Text = _selectedRounds.ToString();
    }

    // ── Выбор таймера ─────────────────────────────────────────────────────────
    private void BtnTimer_Click(object sender, RoutedEventArgs e)
    {
        var btn = (Button)sender;
        _selectedTimer = int.Parse(btn.Tag.ToString()!);

        var activeStyle = (Style)FindResource("BtnCatActive");
        var normalStyle = (Style)FindResource("BtnCat");
        foreach (var (_, b) in _timerButtons) b.Style = normalStyle;
        btn.Style = activeStyle;
    }

    // ── Создать комнату ───────────────────────────────────────────────────────
    private async void BtnCreate_Click(object sender, RoutedEventArgs e)
    {
        TbStatus.Text = "Подключение...";
        try
        {
            if (!_app.SignalR.IsConnected)
                await _app.SignalR.ConnectAsync(_app.ServerUrl);

            AttachHandlers();
            await _app.SignalR.CreateRoom(_app.Nickname, _selectedRounds, _selectedTimer, _selectedCategory);
        }
        catch (Exception ex) { TbStatus.Text = "❌ " + ex.Message; }
    }

    private void AttachHandlers()
    {
        if (_handlersAttached) return;
        _handlersAttached = true;

        _app.SignalR.OnRoomCreated += room =>
            Dispatcher.Invoke(() => _app.Nav.Navigate(new LobbyPage(room, isHost: true)));
        _app.SignalR.OnError += err =>
            Dispatcher.Invoke(() => TbStatus.Text = "❌ " + err);
    }

    private void BtnBack_Click(object sender, RoutedEventArgs e)
        => _app.Nav.Navigate(new MainMenuPage());
}