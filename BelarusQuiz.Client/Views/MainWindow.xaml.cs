// ѕуть: BelarusQuiz.Client/Views/MainWindow.xaml.cs  (ѕќЋЌјя «јћ≈Ќј)

using System.Windows;

namespace BelarusQuiz.Client.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        AppState.Instance.Nav.Initialize(MainFrame);

        // »«ћ≈Ќ≈Ќ»≈: теперь стартуем с LoginPage, а не с MainMenuPage
        MainFrame.Navigate(new LoginPage());
    }
}