using ChatbotLib.DataObjects;
using ChatbotLib.Interfaces;
using System.Text.Json;

namespace ChatbotLib.Services
{
    public class ChatService(IDataSavingService savingService, int limit = 50) : IChatService
    {
        protected static string ChatHistoryFilename => "chathistory.json";
        protected Queue<ChatMessage> messages = [];
        public IEnumerable<ChatMessage> Messages => messages;

        #region Отправка сообщений
        public void SendMessage(ChatMessage message)
        {
            if (messages.Count >= limit)
                messages.Dequeue();
            messages.Enqueue(message);
        }
        public void SendMessage(string author, string message)
            => SendMessage(new() { Author = author, Message = message });
        #endregion
        #region Serialization/Deserialization

        public async Task SaveChatHistory(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            await savingService.SaveDataAsJson(messages, ChatHistoryFilename, token);
        }
        public async Task LoadChatHistory(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            try
            {
                var messages = await savingService.LoadDataAsJson<Queue<ChatMessage>>(ChatHistoryFilename, token);
                if (messages is not null)
                    this.messages = messages;
                else
                    this.messages.Clear();
            }
            catch (Exception ex) when (
                ex is UnauthorizedAccessException ||
                ex is FileNotFoundException ||
                ex is JsonException
            )
            {
                messages.Clear();
            }
        }
        #endregion
    }
}
