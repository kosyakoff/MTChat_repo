using MTChat.Common;
using System;

namespace MTChat.Client
{
    public class ChatServiceCallback : IChatServiceCallback
    {
        #region Public Events

        public event EventHandler<ProxyCallbackEventArgs> ChatCallbackEvent;

        #endregion Public Events

        #region Public Methods

        public void DisconnectByTimeout()
        {
            ProxyCallbackEventArgs args = new ProxyCallbackEventArgs { CallbackType = CallbackType.DisconnectByTimeout };
            ChatCallbackEvent?.Invoke(this, args);
        }

        public void Receive(Person sender, string message)
        {
            ProxyCallbackEventArgs args = new ProxyCallbackEventArgs { CallbackType = CallbackType.Receive, Person = sender, Message = message };
            ChatCallbackEvent?.Invoke(this, args);
        }

        public void ReceiveWhisper(Person sender, string message)
        {
            ProxyCallbackEventArgs args = new ProxyCallbackEventArgs { CallbackType = CallbackType.ReceiveWhisper, Person = sender, Message = message };
            ChatCallbackEvent?.Invoke(this, args);
        }

        public void UserEnter(Person person)
        {
            ProxyCallbackEventArgs args = new ProxyCallbackEventArgs { CallbackType = CallbackType.UserEnter, Person = person };
            ChatCallbackEvent?.Invoke(this, args);
        }

        public void UserLeave(Person person)
        {
            ProxyCallbackEventArgs args = new ProxyCallbackEventArgs { CallbackType = CallbackType.UserLeave, Person = person };
            ChatCallbackEvent?.Invoke(this, args);
        }

        #endregion Public Methods
    }
}