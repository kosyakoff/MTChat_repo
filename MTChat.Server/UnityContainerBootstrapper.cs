using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using MTChat.Server.Services;
using MTChat.Server.ViewModels;

namespace MTChat.Server
{
    /// <summary>
    /// Загрузчик приложения
    /// </summary>
    internal class UnityContainerBootstrapper
    {
        public UnityContainerBootstrapper()
        {
            var unityContainer = new UnityContainer();
            RegisterTypes(unityContainer);
            ServiceLocator.SetLocatorProvider(() => new UnityServiceLocator(unityContainer));
        }

        private void RegisterTypes(IUnityContainer unityContainer)
        {
            // ViewModels
            unityContainer.RegisterType<MainViewModel>();
            
            // Services
            unityContainer.RegisterType(typeof (ILocalChatService), typeof (LocalChatService), new ContainerControlledLifetimeManager());
        }

        public void Run()
        {
            new Views.MainView().ShowDialog();
        }
    }
}
