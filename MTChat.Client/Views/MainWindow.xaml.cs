using System.Windows;

namespace MTChat.Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Public Constructors

        public MainWindow()
        {
            InitializeComponent();

            LogTextBox.TextChanged += (sender, args) => { LogTextBox.ScrollToEnd(); };
        }

        #endregion Public Constructors
    }
}