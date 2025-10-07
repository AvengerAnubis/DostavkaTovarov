using ChatbotLib.DataObjects;
using ChatbotLib.Interfaces;
using System.Text.Json;

namespace ChatbotLib.Services
{
    public class ChatService(IDataSavingService savingService, int limit = 50) : IDisposable, IChatService
    {
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

        protected CancellationTokenSource sharedCts = new();
        public async Task SaveChatHistory(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            var registerToken = token.Register(() => sharedCts.Cancel());

            try
            {
                await savingService.SaveDataAsJson(messages, "chathistory.json", sharedCts.Token);
            }
            finally
            {
                registerToken.Unregister();
            }
        }
        public async Task LoadChatHistory(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            var registerToken = token.Register(() => sharedCts.Cancel());

            try
            {
                var messages = await savingService.LoadDataAsJson<Queue<ChatMessage>>("chathistory.json", sharedCts.Token);
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
            finally
            {
                registerToken.Unregister();
            }
        }
        #endregion

        #region Disposing
        protected bool isDisposed = false;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                isDisposed = true;
                if (disposing)
                {
                    sharedCts.Cancel();
                    sharedCts.Dispose();
                    messages.Clear();
                }
            }
        }
        #endregion
    }
}
