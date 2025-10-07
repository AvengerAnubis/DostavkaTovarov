using ChatbotLib.DataObjects;

namespace ChatbotLib.Interfaces
{
    public interface IChatService
    {
        void SendMessage(ChatMessage message);
        void SendMessage(string author, string message);
        Task SaveChatHistory(CancellationToken token = default);
        Task LoadChatHistory(CancellationToken token = default);
    }
}
