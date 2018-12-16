namespace MTChat.Server.Services
{
    /// <summary>
    /// Интерфейс для управления wcf-сервиса чата
    /// </summary>
    public interface ILocalChatService
    {
        string[] NetworkInterfacesNames();
        void Start(string networkName, string name, int port);
        void Stop();
    }
}