using System.Windows;
using System.Windows.Controls;
using BelarusQuiz.Shared.Enums;

namespace BelarusQuiz.Client.Views;

public partial class CreateRoomPage : Page
{
    private readonly AppState _app = AppState.Instance;

    // Текущий выбор
    private QuizCategory _selectedCategory = QuizCategory.All;
    private int _selectedQuestions = 5;  // ← изменил с Rounds на Questions
    private int _selectedTimer = 15;

    // Словари кнопок для переключения стилей
    private Dictionary<QuizCategory, Button> _catButtons = new();
    private Dictionary<int, Button> _questionButtons = new();  // ← изменил
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
        _questionButtons = new() { { 5, BtnQ5 }, { 10, BtnQ10 }, { 15, BtnQ15 }, { 20, BtnQ20 } };  // ← изменил
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

    // ── Выбор количества вопросов ──────────────────────────────────────────────
    private void BtnQuestions_Click(object sender, RoutedEventArgs e)  // ← изменил
    {
        var btn = (Button)sender;
        _selectedQuestions = int.Parse(btn.Tag.ToString()!);  // ← изменил

        var activeStyle = (Style)FindResource("BtnCatActive");
        var normalStyle = (Style)FindResource("BtnCat");
        foreach (var (_, b) in _questionButtons) b.Style = normalStyle;  // ← изменил
        btn.Style = activeStyle;

        TbSelectedQuestions.Text = _selectedQuestions.ToString();  // ← изменил
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
            await _app.SignalR.CreateRoom(_app.Nickname, _selectedQuestions, _selectedTimer, _selectedCategory);
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