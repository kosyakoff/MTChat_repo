using System.Windows;

namespace MTChat.Server.Views
{
    /// <summary>
    /// Логика взаимодействия для MainView.xaml
    /// </summary>
    public partial class MainView : Window
    {
        public MainView()
        {
            InitializeComponent();

            LogTextBox.TextChanged += (sender, args) => { LogTextBox.ScrollToEnd(); };
        }
    }
}
