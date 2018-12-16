using System.Windows;

namespace MTChat.Server
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            new UnityContainerBootstrapper().Run();
        }
    }
}
