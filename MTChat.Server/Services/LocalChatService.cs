using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.ServiceModel;
using MTChat.WcfService;
using NLog;

namespace MTChat.Server.Services
{
    public class LocalChatService : ILocalChatService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private ServiceHost _host;
        private readonly NetworkInterface[] _networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

        /// <summary>
        /// Запуск сервера
        /// </summary>
        public void Start(string networkName, string name, int port)
        {
            var selectedNetworkInterface = _networkInterfaces.FirstOrDefault(ni => ni.Name == networkName);
            if (selectedNetworkInterface == null)
            {
                throw new InvalidOperationException("Невозможно запустить сервер. Не выбран сетевой интерфейс");
            }

            var addr = selectedNetworkInterface.GetIPProperties().UnicastAddresses.FirstOrDefault(a => a.Address.AddressFamily == AddressFamily.InterNetwork);
            if (addr == null)
            {
                throw new InvalidOperationException("Невозможно запустить сервер. Для выбранного сетевого интерфейса не определен корректный адрес");
            }

            try
            {
                var uri = new Uri($"net.tcp://{addr.Address}:{port}");
                _host = new ServiceHost(typeof(ChatService), uri);
                _host.Open();
                Logger.Info("Сервер '{0}' запущен: {1}", name, uri);
            }
            catch (Exception)
            {
                _host = null;
                throw;
            }
        }

        /// <summary>
        /// Остановка сервера
        /// </summary>
        public void Stop()
        {
            if (_host != null)
            {
                _host.Abort();
                _host.Close();
                _host = null;
            }

            Logger.Info("Сервер остановлен");
        }

        /// <summary>
        /// Имена сетевых адаптеров, зарегистрированных в системе
        /// </summary>
        public string[] NetworkInterfacesNames()
        {
            return _networkInterfaces.Select(ni => ni.Name).ToArray();
        }
    }
}
