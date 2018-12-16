namespace MTChat.Common.Messages
{
    public interface IPersonalMessage : IMessage
    {
        Person To { get; set; }
    }
}
