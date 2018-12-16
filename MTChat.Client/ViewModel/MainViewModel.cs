using CCSWE.Collections.ObjectModel;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using MTChat.Common;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace MTChat.Client.ViewModel
{
    public class MainViewModel : ViewModelBase, IDataErrorInfo
    {
        #region Private Fields

        private static readonly string[] ValidatedProperties =
               {
            "UserName", "ServerIP", "ServerPort"
        };

        private readonly Lazy<RelayCommand> _applicationClosingCommand;

        private readonly Lazy<RelayCommand> _connectCommand;

        private readonly Lazy<RelayCommand> _disconnectCommand;

        private readonly Lazy<RelayCommand> _sendMessageCommand;

        private CommunicationManager _communicationManager;
        private Person _currentPerson;

        private bool _isConnected;

        private bool _isWhispering;

        private string _log;

        private Person _selectedPerson;

        private string _serverIP;
        private string _serverPort;

        private string _userMessageText;

        private string _userName;

        #endregion Private Fields

        #region Public Constructors

        public MainViewModel()
        {
            _communicationManager = new CommunicationManager();

            _connectCommand = new Lazy<RelayCommand>(() => new RelayCommand(Connect, () => IsValid && !IsConnected));
            _disconnectCommand = new Lazy<RelayCommand>(() => new RelayCommand(() => Disconnect(shoudClearInfo: true), () => IsConnected));
            _sendMessageCommand = new Lazy<RelayCommand>(() => new RelayCommand(SendMessage, () => !string.IsNullOrWhiteSpace(UserMessageText)));
            _applicationClosingCommand = new Lazy<RelayCommand>(() => new RelayCommand(ApplicationClosing));
        }

        #endregion Public Constructors

        #region Public Properties

        /// <summary>
        /// ������ ���� ���������� ����
        /// </summary>
        public SynchronizedObservableCollection<Person> AllPersons { get; set; }
            = new SynchronizedObservableCollection<Person>();

        public ICommand ApplicationClosingCommand => _applicationClosingCommand.Value;

        public ICommand ConnectCommand => _connectCommand.Value;

        /// <summary>
        /// �������� ������������ �������
        /// </summary>
        public Person CurrentPerson
        {
            get { return _currentPerson; }
            set { Set(ref _currentPerson, value); }
        }

        public ICommand DisconnectCommand => _disconnectCommand.Value;

        public string Error => null;

        public bool IsConnected
        {
            get { return _isConnected; }
            set { Set(ref _isConnected, value); }
        }

        public bool IsValid => ValidatedProperties.All(property => GetValidationError(property) == null);

        //���� �������� ������� ���������
        public bool IsWhispering
        {
            get { return _isWhispering; }
            set { Set(ref _isWhispering, value); }
        }

        /// <summary>
        /// ��� ����
        /// </summary>
        public string Log
        {
            get { return _log; }
            set { Set(ref _log, value); }
        }

        /// <summary>
        /// ��������� �������� ����
        /// </summary>
        public Person SelectedPerson
        {
            get { return _selectedPerson; }
            set { Set(ref _selectedPerson, value); }
        }

        public ICommand SendMessageCommand => _sendMessageCommand.Value;

        public string ServerIP
        {
            get { return _serverIP; }
            set { Set(ref _serverIP, value); }
        }

        public string ServerPort
        {
            get { return _serverPort; }
            set { Set(ref _serverPort, value); }
        }

        public string UserMessageText
        {
            get { return _userMessageText; }
            set { Set(ref _userMessageText, value); }
        }

        /// <summary>
        /// ��� ��������� ������������ �������
        /// </summary>
        public string UserName
        {
            get { return _userName; }
            set { Set(ref _userName, value); }
        }

        #endregion Public Properties

        #region Public Indexers

        public string this[string propertyName] => GetValidationError(propertyName);

        #endregion Public Indexers

        #region Internal Methods

        /// <summary>
        /// ���������� ��������� ������
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void Callback_ChatCallbackEvent(object sender, ProxyCallbackEventArgs e)
        {
            switch (e.CallbackType)
            {
                case CallbackType.Receive:
                    LogMessage($"{e.Person.Name}: {e.Message}");
                    break;

                case CallbackType.ReceiveWhisper:

                    if (e.Person == null)
                        return;

                    LogMessage($"{e.Person.Name} ������: {e.Message}");
                    break;

                case CallbackType.UserEnter:
                    LogMessage($"{e.Person.Name} ����� � ���");
                    AddPersonToPersonList(e.Person);
                    break;

                case CallbackType.UserLeave:
                    if (e.Person == CurrentPerson)
                        return;

                    RemovePersonFromPersonList(e.Person);
                    LogMessage($"{e.Person.Name} ����� �� ����");
                    break;

                case CallbackType.DisconnectByTimeout:
                    LogMessage("�� ���� ��������� �� ��������");
                    break;

                default:
                    break;
            }
        }

        internal void Disconnect(bool shoudClearInfo = false)
        {
            _communicationManager.Disconnect();

            //��������� ���������� �� �������
            if (shoudClearInfo)
            {
                CurrentPerson = null;
                SelectedPerson = null;
                AllPersons.Clear();
            }

            IsConnected = false;
        }

        /// <summary>
        /// ����� ������, ���������� ��������� ������ �� ������
        /// </summary>
        /// <param name="opRes"></param>
        internal void LogError(OperationResult opRes)
        {
            if (opRes.Errors.Any())
                LogError(opRes.Errors.ToList().Last().ErrorMessage);
        }

        internal void LogError(string message)
        {
            MessageBox.Show(message, "������", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        #endregion Internal Methods

        #region Private Methods

        private void AddPersonToPersonList(Person person)
        {
            if (person != null && !AllPersons.Contains(person))
                AllPersons.Add(person);
        }

        /// <summary>
        /// ���������� �������� ����������
        /// </summary>
        private void ApplicationClosing()
        {
            Disconnect();
        }

        /// <summary>
        /// ����������� � �������
        /// </summary>
        private void Connect()
        {
            Person person = new Person { Name = UserName };

            try
            {
                var callback = new ChatServiceCallback();
                callback.ChatCallbackEvent += Callback_ChatCallbackEvent;

                var joinResult = _communicationManager.Connect(serverIP: ServerIP, serverPort: ServerPort, person: person, callback: callback);

                if (!joinResult.Success)
                {
                    Disconnect();
                    LogError(joinResult);
                    return;
                }

                //�������� �������� ������������ �������
                CurrentPerson = person;

                AllPersons.Clear();
                foreach (var per in joinResult.Result.OrderBy(x => x.Name))
                    AllPersons.Add(per);

                IsConnected = true;
            }
            catch (Exception ex)
            {
                LogError("�� ���������� ������������ � �������: " + ex.Message);
                IsConnected = false;
            }
        }

        private string GetValidationError(string propertyName)
        {
            if (Array.IndexOf(ValidatedProperties, propertyName) < 0)
                return null;

            string error = null;

            switch (propertyName)
            {
                case "UserName": error = ValidateUserName(); break;
                case "ServerPort": error = ValidateServerPort(); break;
                case "ServerIP": error = ValidateServerIP(); break;
            }

            return error;
        }

        private void LogMessage(string message)
        {
            Log += $"[{DateTime.Now}] {message} {Environment.NewLine}";
        }

        private void RemovePersonFromPersonList(Person person)
        {
            if (person != null)
                AllPersons.Remove(person);
        }

        /// <summary>
        /// ������� ���������
        /// </summary>
        private void SendMessage()
        {
            if (IsWhispering)
            {
                if (SelectedPerson == null)
                {
                    LogError("�� ������ ������������ ��� �������� ���������");
                    return;
                }

                if (CurrentPerson.Equals(SelectedPerson))
                {
                    LogError("������ ���������� ������ ��������� ������ ����");
                    return;
                }
            }

            Common.Messages.TextMessage mess = IsWhispering ?
                new Common.Messages.PersonalTextMessage(CurrentPerson, SelectedPerson, UserMessageText) :
                new Common.Messages.TextMessage(CurrentPerson, UserMessageText);

            OperationResult opRes = new OperationResult { Success = true };

            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                try
                {
                    opRes = _communicationManager.SendMessage(IsWhispering, mess);
                }
                catch (Exception ex)
                {
                    opRes.Success = false;
                    OperationError err = new OperationError();

                    ///���� ��� ������� ��������� �������������� ������ ���������� � �������� ��������� �����������
                    if (ex.GetType() == typeof(System.ServiceModel.CommunicationObjectFaultedException))
                    {
                        err.ErrorMessage = "�� ���������� ����������� � ��������";
                        Disconnect(shoudClearInfo: true);
                    }
                    else
                        err.ErrorMessage = ex.Message;

                    opRes.Errors.Add(err);
                }

                if (!opRes.Success)
                {
                    LogError(opRes);
                }
            });

            UserMessageText = string.Empty;
        }

        private string ValidateServerIP()
        {
            string errString = "�� ����� ���������� IP ����� �������";
            if (String.IsNullOrWhiteSpace(ServerIP))
            {
                return errString;
            }

            string[] splitValues = ServerIP.Split('.');
            if (splitValues.Length != 4)
            {
                return errString;
            }

            byte tempForParsing;

            return splitValues.All(r => byte.TryParse(r, out tempForParsing)) ? null : errString;
        }

        private string ValidateServerPort()
        {
            int val;
            return int.TryParse(ServerPort, out val) && val > 0 && val < 65536 ? null : "������ ������������ ����";
        }

        private string ValidateUserName()
        {
            return String.IsNullOrEmpty(UserName) ? "��� ������������ �� ����� ���� ������" : null;
        }

        #endregion Private Methods
    }
}