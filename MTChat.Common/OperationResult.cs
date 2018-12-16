using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MTChat.Common
{
    /// <summary>
    /// Возвращаемый класс для всех методов сервера
    /// </summary>
    [DataContract]
    public class OperationResult
    {
        public OperationResult()
        {
            Errors = new List<OperationError>();
        }

        [DataMember]
        public bool Success { get; set; }

        [DataMember]
        public IList<OperationError> Errors { get; set; }
    }

    [DataContract]
    public class OperationResult<T> : OperationResult
    {
        [DataMember]
        public T Result { get; set; }
    }
}
