using System.Runtime.Serialization;

namespace MTChat.Common
{
    [DataContract]
    public class OperationError
    {
        [DataMember]
        public string ErrorMessage { get; set; }
    }
}
