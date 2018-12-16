using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using MTChat.Server.Helpers;
using MTChat.Server.Services;
using NLog;

namespace MTChat.Server.ViewModels
{
    public class MainViewModel : ViewModelBase, IDataErrorInfo
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly ILocalChatService _localChatService;

        public MainViewModel(ILocalChatService localChatService)
        {
            _localChatService = localChatService;
            _startCommand = new Lazy<RelayCommand>(() => new RelayCommand(Start, () => IsValid && !IsRunning));
            _stopCommand = new Lazy<RelayCommand>(() => new RelayCommand(Stop, () => IsRunning));
            _clearLogCommand = new Lazy<RelayCommand>(() => new RelayCommand(ClearLog));

            NetworkInterfacesNames = _localChatService.NetworkInterfacesNames();
            InitNLog();
        }

        #region Properties

        private string _serverName;

        public string ServerName
        {
            get { return _serverName; }
            set { Set(ref _serverName, value); }
        }

        private string _serverPort;
        public string ServerPort
        {
            get { return _serverPort; }
            set { Set(ref _serverPort, value); }
        }

        private string _log;
        public string Log
        {
            get { return _log; }
            set { Set(ref _log, value); }
        }

        public string[] NetworkInterfacesNames { get; }

        private string _selectedNetworkInterfaceName;
        public string SelectedNetworkInterfaceName
        {
            get { return _selectedNetworkInterfaceName; }
            set { Set(ref _selectedNetworkInterfaceName, value); }
        }

        private bool _isRunning;
        public bool IsRunning
        {
            get { return _isRunning; }
            set { Set(ref _isRunning, value); }
        }

        #endregion

        #region Commands

        private readonly Lazy<RelayCommand> _startCommand;
        public ICommand StartCommand => _startCommand.Value;

        private void Start()
        {
            var port = int.Parse(ServerPort);
            try
            {
                _localChatService.Start(SelectedNetworkInterfaceName, ServerName, port);
                IsRunning = true;
            }
            catch (Exception e)
            {
                Logger.Error(e, e.Message);
                IsRunning = false;
            }
        }

        private readonly Lazy<RelayCommand> _stopCommand;
        public ICommand StopCommand => _stopCommand.Value;

        private void Stop()
        {
            _localChatService.Stop();
            IsRunning = false;
        }

        private readonly Lazy<RelayCommand> _clearLogCommand;
        public ICommand ClearLogCommand => _clearLogCommand.Value;

        private void ClearLog()
        {
            Log = String.Empty;
        }

        #endregion

        #region Nlog

        /// <summary>
        /// Инициализатор цели для логгера
        /// </summary>
        private void InitNLog()
        {
            var target = LogManager.Configuration.AllTargets.FirstOrDefault(t => t is NLogViewerTarget) as NLogViewerTarget;
            if (target != null)
            {
                target.LogReceived += OnLogReceived;
            }
        }

        private void OnLogReceived(string logMessage)
        {
            Log += $"{logMessage}";
        }

        #endregion

        #region IDataErrorInfo members

        public string this[string propertyName] => GetValidationError(propertyName);

        public string Error => null;

        #endregion

        #region Validation

        public bool IsValid => ValidatedProperties.All(property => GetValidationError(property) == null);

        private static readonly string[] ValidatedProperties =
        {
            "ServerName", "ServerPort", "SelectedNetworkInterfaceName"
        };

        private string GetValidationError(string propertyName)
        {
            if (Array.IndexOf(ValidatedProperties, propertyName) < 0)
                return null;

            string error = null;

            switch (propertyName)
            {
                case "ServerName": error = ValidateServerName(); break;
                case "ServerPort": error = ValidateServerPort(); break;
                case "SelectedNetworkInterfaceName": error = ValidateNetworkInterface(); break;
            }

            return error;
        }

        private string ValidateServerName()
        {
            return String.IsNullOrEmpty(ServerName) ? "Имя сервера не может быть пустым" : null;
        }

        private string ValidateNetworkInterface()
        {
            return String.IsNullOrEmpty(SelectedNetworkInterfaceName) ? "Не выбран сетевой интерфейс" : null;
        }

        private string ValidateServerPort()
        {
            int val;
            return int.TryParse(ServerPort, out val) && val > 0 && val < 65536 ? null : "Указан некорректный порт";
        }

        #endregion

    }
}