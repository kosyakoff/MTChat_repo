using System.ServiceModel;
using MTChat.Common.Messages;

namespace MTChat.Common
{
    [ServiceContract(CallbackContract = typeof(IChatServiceCallback), SessionMode = SessionMode.Required)]
    public interface IChatService
    {
        [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
        OperationResult Say(TextMessage msg);

        [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
        OperationResult Whisper(PersonalTextMessage msg);

        [OperationContract(IsOneWay = false, IsInitiating = true, IsTerminating = false)]
        OperationResult<Person[]> Join(Person person);

        [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = true)]
        OperationResult Leave();
    }
}
