namespace MTChat.Common.Messages
{
    public interface ITextMessage : IMessage
    {
        string Text { get; set; }
    }
}
