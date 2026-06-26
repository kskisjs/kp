using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace BelarusQuiz.Client.Views;

public partial class SoloGamePage : Page
{
    private readonly AppState _app = AppState.Instance;

    private static readonly List<(string Text, string Category, string[] Options, int Correct, string Explanation)> _questions = new()
    {
        ("Какой город является столицей Республики Беларусь?", "🗺️ География",
            new[]{"Минск","Гомель","Брест","Гродно"}, 0, "Минск — столица и крупнейший город Беларуси."),
        ("В каком году Беларусь провозгласила независимость?", "📜 История",
            new[]{"1990","1991","1992","1989"}, 1, "25 августа 1991 года провозглашена независимость Беларуси."),
        ("Как называется главная река Беларуси?", "🌿 Природа",
            new[]{"Неман","Припять","Днепр","Западная Двина"}, 2, "Днепр — крупнейшая река Беларуси."),
        ("Какой замок изображён на купюре 20 рублей?", "🎭 Культура",
            new[]{"Несвижский","Мирский","Гродненский","Лидский"}, 1, "Мирский замок — объект ЮНЕСКО на купюре 20 рублей."),
        ("Как называется крупнейший лесной массив Беларуси?", "🌿 Природа",
            new[]{"Беловежская пуща","Налибокская пуща","Припятский лес","Брестский бор"}, 0,
            "Беловежская пуща — древнейший лесной массив Европы."),
        ("Кто первым из белорусов полетел в космос?", "📜 История",
            new[]{"Пётр Климук","Владимир Коваленок","Олег Новицкий","Марина Василевская"}, 0,
            "Пётр Климук — первый белорусский космонавт, 1973 год."),
        ("Как называется традиционный белорусский картофельный блин?", "🎭 Культура",
            new[]{"Бабка","Драник","Колдуны","Мачанка"}, 1, "Драники — национальный символ белорусской кухни."),
        ("Какое озеро является самым большим в Беларуси?", "🌿 Природа",
            new[]{"Нарочь","Освейское","Лукомское","Дривяты"}, 0, "Нарочь — крупнейшее озеро Беларуси, ~80 км²."),
        ("В каком городе родился Марк Шагал?", "🎭 Культура",
            new[]{"Минск","Гомель","Витебск","Могилёв"}, 2, "Марк Шагал родился в Витебске в 1887 году."),
        ("Сколько областей в Республике Беларусь?", "🗺️ География",
            new[]{"5","6","7","8"}, 1, "В Беларуси 6 областей."),
        ("Какой белорусский город называют «городом над Неманом»?", "🗺️ География",
            new[]{"Брест","Гродно","Лида","Новогрудок"}, 1, "Гродно стоит на реке Неман."),
        ("Когда отмечается День Независимости Беларуси?", "📜 История",
            new[]{"25 марта","3 июля","27 июля","25 августа"}, 1, "3 июля — день освобождения Минска в 1944 году."),
        ("Какой зверь является символом Беловежской пущи?", "🌿 Природа",
            new[]{"Волк","Медведь","Зубр","Лось"}, 2, "Зубр — крупнейшее наземное млекопитающее Европы."),
        ("Кто написал роман «Знак беды»?", "🎭 Культура",
            new[]{"Янка Купала","Якуб Колас","Василь Быков","Иван Мележ"}, 2, "Василь Быков — выдающийся белорусский писатель."),
        ("Какова длина государственной границы Беларуси (примерно)?", "🗺️ География",
            new[]{"1500 км","2300 км","3100 км","800 км"}, 2, "Государственная граница Беларуси составляет около 3107 км."),
        ("Кто возглавил восстание 1863–1864 годов на белорусских землях?", "📜 История",
            new[]{"Кастусь Калиновский","Тадеуш Костюшко","Франциск Скорина","Якуб Колас"}, 0,
            "Кастусь Калиновский — белорусский революционер."),
        ("В каком году был основан Минск?", "📜 История",
            new[]{"980","1000","1067","1147"}, 2,
            "Минск впервые упоминается в летописях в 1067 году."),
        ("Кто напечатал первую книгу на старобелорусском языке?", "📜 История",
            new[]{"Франциск Скорина","Пётр Мстиславец","Симеон Полоцкий","Кирилл Туровский"}, 0,
            "Франциск Скорина в 1517 году издал «Псалтырь»."),
        ("Как называется главный проспект Минска?", "🗺️ География",
            new[]{"Проспект Победы","Проспект Независимости","Проспект Мира","Проспект Ленина"}, 1,
            "Проспект Независимости — центральная магистраль Минска, более 15 км."),
        ("Через какой город проходит «нулевой меридиан» Восточной Европы?", "🗺️ География",
            new[]{"Брест","Гродно","Пинск","Лида"}, 0,
            "Через Брест проходит меридиан 23°42'."),
    };

    private List<int> _shuffledIdx = new();
    private int _currentQ;
    private int _myScore;
    private int _botScore;
    private int _streak;
    private int _timeLeft;
    private const int TimerSeconds = 15;
    private DispatcherTimer? _timer;
    private readonly List<Button> _buttons;
    private readonly Random _rng = new();
    private bool _answered;

    public SoloGamePage()
    {
        InitializeComponent();
        _buttons = new() { Btn0, Btn1, Btn2, Btn3 };
        TbMyName.Text = AppState.Instance.Nickname;
        StartGame();
    }

    private void StartGame()
    {
        // Берём 10 случайных вопросов из расширенного банка
        _shuffledIdx = Enumerable.Range(0, _questions.Count).OrderBy(_ => _rng.Next()).Take(10).ToList();
        _currentQ = _myScore = _botScore = _streak = 0;
        UpdateScores();
        NextQuestion();
    }

    private void NextQuestion()
    {
        if (_currentQ >= _shuffledIdx.Count) { EndGame(); return; }

        _answered = false;
        var q = _questions[_shuffledIdx[_currentQ]];
        TbRound.Text = $"Вопрос {_currentQ + 1} из {_shuffledIdx.Count}";
        TbCategory.Text = q.Category;
        TbQuestion.Text = q.Text;
        ResultPanel.Visibility = Visibility.Collapsed;

        for (int i = 0; i < _buttons.Count; i++)
        {
            _buttons[i].Content = i < q.Options.Length ? q.Options[i] : "";
            _buttons[i].IsEnabled = i < q.Options.Length;
            _buttons[i].Background = new SolidColorBrush(Color.FromRgb(0x1E, 0x38, 0x20));
            _buttons[i].BorderBrush = new SolidColorBrush(Color.FromRgb(0x2A, 0x4A, 0x2C));
            _buttons[i].Foreground = new SolidColorBrush(Colors.White);
        }

        _timeLeft = TimerSeconds;
        TimerBar.Maximum = TimerSeconds;
        TimerBar.Value = TimerSeconds;
        TbTimer.Text = TimerSeconds.ToString();
        TbTimer.Foreground = new SolidColorBrush(Color.FromRgb(0x2E, 0xCC, 0x40));

        _timer?.Stop();
        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += Timer_Tick;
        _timer.Start();
    }

    private void Timer_Tick(object? s, EventArgs e)
    {
        _timeLeft--;
        TbTimer.Text = _timeLeft.ToString();
        TimerBar.Value = _timeLeft;
        TbTimer.Foreground = _timeLeft <= 5
            ? new SolidColorBrush(Color.FromRgb(0xE7, 0x4C, 0x3C))
            : new SolidColorBrush(Color.FromRgb(0x2E, 0xCC, 0x40));

        if (_timeLeft <= 0) { _timer!.Stop(); EvaluateAnswer(-1); }
    }

    private void Answer_Click(object sender, RoutedEventArgs e)
    {
        if (_answered) return;
        _timer?.Stop();
        int idx = int.Parse(((Button)sender).Tag.ToString()!);
        EvaluateAnswer(idx);
    }

    private async void EvaluateAnswer(int chosen)
    {
        _answered = true;
        foreach (var b in _buttons) b.IsEnabled = false;

        var q = _questions[_shuffledIdx[_currentQ]];
        bool correct = chosen == q.Correct;

        if (chosen >= 0)
            _buttons[chosen].Background = correct
                ? new SolidColorBrush(Color.FromRgb(0x1A, 0x6E, 0x30))
                : new SolidColorBrush(Color.FromRgb(0x7A, 0x1A, 0x1A));
        _buttons[q.Correct].Background = new SolidColorBrush(Color.FromRgb(0x1A, 0x6E, 0x30));
        _buttons[q.Correct].BorderBrush = new SolidColorBrush(Color.FromRgb(0x2E, 0xCC, 0x40));

        if (correct)
        {
            _streak++;
            double ratio = (double)_timeLeft / TimerSeconds;
            _myScore += 100 + (int)(ratio * 50) + Math.Min(_streak * 10, 50);
        }
        else _streak = 0;

        bool botCorrect = _rng.NextDouble() < 0.65;
        if (botCorrect) _botScore += _rng.Next(80, 151);

        UpdateScores();

        TbResult.Text = correct ? "✔ Правильно!" : chosen < 0 ? "⏱ Время вышло!" : "✖ Неверно";
        TbResult.Foreground = correct
            ? new SolidColorBrush(Color.FromRgb(0x2E, 0xCC, 0x40))
            : new SolidColorBrush(Color.FromRgb(0xE7, 0x4C, 0x3C));
        TbExplanation.Text = q.Explanation;
        ResultPanel.Visibility = Visibility.Visible;

        await Task.Delay(3000);
        _currentQ++;
        NextQuestion();
    }

    private void UpdateScores()
    {
        TbMyScore.Text = _myScore.ToString();
        TbBotScore.Text = _botScore.ToString();
        TbStreak.Text = _streak >= 2 ? $"🔥 Серия: {_streak}" : "";
    }

    // ── КОНЕЦ ИГРЫ: сохраняем статистику ─────────────────────────────────────
    private async void EndGame()
    {
        _timer?.Stop();
        bool won = _myScore >= _botScore;

        // ★ ИСПРАВЛЕНИЕ: сохраняем статистику на сервере и в AppState
        if (_app.IsLoggedIn)
        {
            await _app.Http.SaveGameAsync(
                login: _app.UserLogin,
                won: won,
                myScore: _myScore,
                opponentScore: _botScore,
                opponentNickname: "🤖 Бот",
                category: "Все категории",
                rounds: _shuffledIdx.Count
            );

            // Обновляем локально сразу, не ждём перезапроса
            _app.UserGamesPlayed++;
            if (won) _app.UserWins++;
            _app.UserTotalScore += _myScore;
        }

        var result = new BelarusQuiz.Shared.Models.GameResultDto
        {
            WinnerNickname = won ? _app.Nickname : "🤖 Бот",
            XpEarned = 30 + (won ? 20 : 0),
            FinalScores = new()
            {
                [_app.Nickname] = _myScore,
                ["🤖 Бот"] = _botScore
            }
        };
        _app.Nav.Navigate(new ResultsPage(result, _app.Nickname));
    }
}