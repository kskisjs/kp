using System.Windows.Controls;

namespace BelarusQuiz.Client.Services;

public class NavigationService
{
    private Frame? _frame;

    public void Initialize(Frame frame) => _frame = frame;

    public void Navigate(Page page) => _frame?.Navigate(page);

    public void GoBack() => _frame?.GoBack();
}
