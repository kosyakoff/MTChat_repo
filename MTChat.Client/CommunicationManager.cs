using MTChat.Common;
using System;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace MTChat.Client
{
    /// <summary>
    /// Менеджер по работе с сервисом
    /// </summary>
    public class CommunicationManager
    {
        #region Private Fields

        private DuplexChannelFactory<IChatService> _factory;
        private IChatService _service;

        #endregion Private Fields

        #region Public Methods

        /// <summary>
        /// Соединяемся с сервисом
        /// </summary>
        /// <param name="serverIP"></param>
        /// <param name="serverPort"></param>
        /// <param name="person"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public OperationResult<Person[]> Connect(string serverIP, string serverPort, Person person, ChatServiceCallback callback)
        {
            //Создается соединение с сервисом
            Binding binding = new System.ServiceModel.NetTcpBinding();
            var uri = new Uri($"net.tcp://{serverIP}:{serverPort}");
            EndpointAddress address = new EndpointAddress(uri);

            _factory = new DuplexChannelFactory<IChatService>(callback, binding, address);

            _service = _factory.CreateChannel();

            if (_service == null)
                throw new Exception("Не удалось создать соединение с сервисом");

            OperationResult<Person[]> joinResult = _service.Join(person);

            return joinResult;
        }

        /// <summary>
        /// Отключение от сервиса
        /// </summary>
        public void Disconnect()
        {
            try
            {
                if (_service != null && _factory.State == CommunicationState.Opened)
                {
                    _service.Leave();
                    _service = null;
                }
            }
            catch
            {
            }
            finally
            {
                if (_factory != null && _factory.State == CommunicationState.Opened)
                {
                    _factory.Abort();
                    _factory.Close();
                    _factory = null;
                }
            }
        }

        /// <summary>
        /// Отсылка сообщение к серверу
        /// </summary>
        /// <param name="isWhispering"></param>
        /// <param name="mess"></param>
        /// <returns></returns>
        public OperationResult SendMessage(bool isWhispering, Common.Messages.TextMessage mess)
        {
            OperationResult opRes = new OperationResult();

            //Если отправлется личное сообщение
            if (isWhispering)
            {
                opRes = _service.Whisper(mess as Common.Messages.PersonalTextMessage);
            }
            //Сообщение в общий чат
            else
                opRes = _service.Say(mess);

            return opRes;
        }

        #endregion Public Methods
    }
}