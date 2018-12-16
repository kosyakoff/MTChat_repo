using System;

namespace MTChat.Common
{
    public class ProxyCallbackEventArgs : EventArgs
    {
        public CallbackType CallbackType;
        public Person Person;
        public string Message;
    }
}
