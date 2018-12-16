using System;
using System.ServiceModel;

namespace MTChat.Common
{
    [ServiceContract]
    public interface IChatServiceCallback
    {
        [OperationContract(IsOneWay = true)]
        void Receive(Person sender, string message);

        [OperationContract(IsOneWay = true)]
        void ReceiveWhisper(Person sender, string message);

        [OperationContract(IsOneWay = true)]
        void UserEnter(Person person);

        [OperationContract(IsOneWay = true)]
        void UserLeave(Person person);

        [OperationContract]
        void DisconnectByTimeout();

        event EventHandler<ProxyCallbackEventArgs> ChatCallbackEvent;
    }
}
