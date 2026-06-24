using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using BelarusQuiz.Shared.Models;

namespace BelarusQuiz.Client.Views;

public partial class GamePage : Page
{
    private readonly AppState _app = AppState.Instance;
    private readonly string _myNickname;
    private readonly List<Button> _answerButtons;
    private QuestionDto? _currentQuestion;
    private DateTime _questionStartTime;
    private int _timerMax = 15;

    public GamePage(string myNickname)
    {
        InitializeComponent();
        _myNickname = myNickname;
        _answerButtons = new() { Btn0, Btn1, Btn2, Btn3 };
        TbMyName.Text = myNickname;
        SubscribeEvents();
    }

    private void SubscribeEvents()
    {
        _app.SignalR.OnQuestionReceived += q => Dispatcher.Invoke(() => ShowQuestion(q));
        _app.SignalR.OnTimerTick += t => Dispatcher.Invoke(() => UpdateTimer(t));
        _app.SignalR.OnRoundResult += r => Dispatcher.Invoke(() => ShowRoundResult(r));
        _app.SignalR.OnGameFinished += r => Dispatcher.Invoke(() =>
            _app.Nav.Navigate(new ResultsPage(r, _myNickname)));
    }

    private void ShowQuestion(QuestionDto q)
    {
        _currentQuestion = q;
        _questionStartTime = DateTime.UtcNow;
        _timerMax = 15;

        TbRound.Text = $"Раунд {q.RoundNumber} из {q.TotalRounds}";
        TbCategory.Text = q.Category;
        TbQuestion.Text = q.Text;

        for (int i = 0; i < _answerButtons.Count; i++)
        {
            var btn = _answerButtons[i];
            btn.Content = i < q.Options.Count ? q.Options[i] : "";
            btn.IsEnabled = i < q.Options.Count;
            btn.Background = new SolidColorBrush(Color.FromRgb(0x1E, 0x38, 0x20));
            btn.BorderBrush = new SolidColorBrush(Color.FromRgb(0x2A, 0x4A, 0x2C));
            btn.Foreground = new SolidColorBrush(Colors.White);
        }

        RoundResultPanel.Visibility = Visibility.Collapsed;
        TimerBar.Maximum = 100;
        TimerBar.Value = 100;
    }

    private void UpdateTimer(int seconds)
    {
        TbTimer.Text = seconds.ToString();
        TimerBar.Value = (double)seconds / _timerMax * 100;
        TbTimer.Foreground = seconds <= 5
            ? new SolidColorBrush(Color.FromRgb(0xE7, 0x4C, 0x3C))
            : new SolidColorBrush(Color.FromRgb(0x2E, 0xCC, 0x40));
    }

    private async void Answer_Click(object sender, RoutedEventArgs e)
    {
        if (_currentQuestion == null) return;
        var btn = (Button)sender;
        int idx = int.Parse(btn.Tag.ToString()!);

        // Выделить выбранный
        btn.Background = new SolidColorBrush(Color.FromRgb(0x27, 0x6B, 0x3A));
        btn.BorderBrush = new SolidColorBrush(Color.FromRgb(0x2E, 0xCC, 0x40));

        // Заблокировать все кнопки
        foreach (var b in _answerButtons) b.IsEnabled = false;

        var elapsed = (long)(DateTime.UtcNow - _questionStartTime).TotalMilliseconds;
        await _app.SignalR.SubmitAnswer(new AnswerDto
        {
            QuestionId = _currentQuestion.Id,
            AnswerIndex = idx,
            TimeMs = elapsed
        });
    }

    private void ShowRoundResult(RoundResultDto result)
    {
        // Подсветить правильный ответ зелёным
        for (int i = 0; i < _answerButtons.Count; i++)
        {
            _answerButtons[i].IsEnabled = false;
            if (i == result.CorrectIndex)
            {
                _answerButtons[i].Background = new SolidColorBrush(Color.FromRgb(0x1A, 0x6E, 0x30));
                _answerButtons[i].BorderBrush = new SolidColorBrush(Color.FromRgb(0x2E, 0xCC, 0x40));
            }
        }

        // ИСПРАВЛЕНИЕ: используем MyConnectionId для точного определения нашего результата
        var myId = AppState.Instance.MyConnectionId;

        // Обновить счёт — теперь правильно: ищем по ConnectionId, а не по индексу
        foreach (var entry in result.Scores)
        {
            if (entry.Key == myId)
                TbMyScore.Text = entry.Value.ToString();
            else
                TbOppScore.Text = entry.Value.ToString();
        }

        // ИСПРАВЛЕНИЕ: определяем наш ответ по ConnectionId, а не по индексу
        bool myCorrect = result.Answers.TryGetValue(myId, out var correct) && correct;

        TbResult.Text = myCorrect ? "✔ Правильно!" : "✖ Неверно";
        TbResult.Foreground = myCorrect
            ? new SolidColorBrush(Color.FromRgb(0x2E, 0xCC, 0x40))
            : new SolidColorBrush(Color.FromRgb(0xE7, 0x4C, 0x3C));

        TbExplanation.Text = result.Explanation ?? "";
        RoundResultPanel.Visibility = Visibility.Visible;
    }
}