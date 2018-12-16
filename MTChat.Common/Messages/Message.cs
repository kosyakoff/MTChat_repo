using System.Runtime.Serialization;

namespace MTChat.Common.Messages
{
    [DataContract]
    public abstract class Message : IMessage
    {
        [DataMember]
        public Person From { get; set; }
    }
}
