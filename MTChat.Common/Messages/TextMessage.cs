using System.Runtime.Serialization;

namespace MTChat.Common.Messages
{
    [DataContract]
    public class TextMessage : Message, ITextMessage
    {
        public TextMessage(Person from, string text)
        {
            From = from;
            Text = text;
        }

        [DataMember]
        public string Text { get; set; }
    }
}
