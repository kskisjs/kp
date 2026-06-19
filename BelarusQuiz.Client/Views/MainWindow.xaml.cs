using System.Windows;

namespace BelarusQuiz.Client.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        AppState.Instance.Nav.Initialize(MainFrame);
        MainFrame.Navigate(new MainMenuPage());
    }
}
