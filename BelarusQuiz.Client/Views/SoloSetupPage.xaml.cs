using System.Windows;
using System.Windows.Controls;

namespace BelarusQuiz.Client.Views;

public partial class SoloSetupPage : Page
{
    private string _selectedCategory = "All";
    private int _selectedCount = 5;

    private Dictionary<string, Button> _catBtns = new();
    private Dictionary<int, Button> _countBtns = new();

    public SoloSetupPage()
    {
        InitializeComponent();
        Loaded += (_, _) => Init();
    }

    private void Init()
    {
        _catBtns = new()
        {
            { "All",       BtnCatAll  },
            { "Geography", BtnCatGeo  },
            { "History",   BtnCatHist },
            { "Culture",   BtnCatCult },
            { "Nature",    BtnCatNat  },
            { "Symbols",   BtnCatSym  },
            { "People",    BtnCatPpl  },
        };
        _countBtns = new() { { 5, BtnQ5 }, { 10, BtnQ10 }, { 15, BtnQ15 }, { 20, BtnQ20 } };
    }

    private void BtnCat_Click(object sender, RoutedEventArgs e)
    {
        var btn = (Button)sender;
        _selectedCategory = btn.Tag?.ToString() ?? "All";

        var active = (Style)FindResource("BtnCatActive");
        var normal = (Style)FindResource("BtnCat");
        foreach (var (_, b) in _catBtns) b.Style = normal;
        btn.Style = active;

        TbSelectedCat.Text = _selectedCategory switch
        {
            "Geography" => "🗺️ География",
            "History" => "📜 История",
            "Culture" => "🎭 Культура",
            "Nature" => "🌿 Природа",
            "Symbols" => "🏛️ Символика",
            "People" => "🧑 Знаменитости",
            _ => "Все категории"
        };
    }

    private void BtnCount_Click(object sender, RoutedEventArgs e)
    {
        var btn = (Button)sender;
        _selectedCount = int.Parse(btn.Tag.ToString()!);

        var active = (Style)FindResource("BtnNumActive");
        var normal = (Style)FindResource("BtnNum");
        foreach (var (_, b) in _countBtns) b.Style = normal;
        btn.Style = active;

        TbSelectedCount.Text = $"{_selectedCount} вопросов";
    }

    private void BtnStart_Click(object sender, RoutedEventArgs e)
        => AppState.Instance.Nav.Navigate(new SoloGamePage(_selectedCategory, _selectedCount));

    private void BtnBack_Click(object sender, RoutedEventArgs e)
        => AppState.Instance.Nav.Navigate(new MainMenuPage());
}