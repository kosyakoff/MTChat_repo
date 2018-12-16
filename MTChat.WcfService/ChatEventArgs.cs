using MTChat.Common;

namespace MTChat.WcfService
{
    public class ChatEventArgs
    {
        public CallbackType CallbackType;
        public Person Person;
        public string Message;
    }
}
