using System.Runtime.Serialization;

namespace MTChat.Common.Messages
{
    [DataContract]
    public class PersonalTextMessage : TextMessage, IPersonalMessage
    {
        public PersonalTextMessage(Person from, Person to, string text)
            : base(from, text)
        {
            To = to;
        }

        [DataMember]
        public Person To { get; set; }
    }
}
