using System.IO.Pipelines;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace ChatbotLib
{
    public class ChatService(DataSavingService savingService) : IDisposable
    {
        public List<ChatMessage> Messages { get; protected set; } = [];

        #region Отправка сообщений
        public void SendMessage(ChatMessage message) => Messages.Add(message);
        public void SendMessage(string author, string message) 
            => SendMessage(new() { Author = author, Message = message });
        #endregion
        #region Serialization/Deserialization
        
        protected CancellationTokenSource sharedCts = new();
        public async Task SaveToJson(CancellationToken token = default)
        {
            var registerToken = token.Register(() => sharedCts.Cancel());

            try
            {
                await savingService.SaveDataAsJson(Messages, "chathistory.json", sharedCts.Token);
            }
            finally
            {
                registerToken.Unregister();
            }
        }
        public async Task LoadFromJson(CancellationToken token = default)
        {
            var registerToken = token.Register(() => sharedCts.Cancel());

            try
            {
                var messages = await savingService.LoadDataAsJson<List<ChatMessage>>("chathistory.json", sharedCts.Token);
                if (messages is not null)
                    Messages = messages;
                else
                    Messages.Clear();
            }
            catch (Exception ex) when (
                ex is UnauthorizedAccessException ||
                ex is FileNotFoundException ||
                ex is JsonException
            )
            {
                Messages.Clear();
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
        protected void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                if (disposing)
                {
                    sharedCts.Cancel();
                    sharedCts.Dispose();
                    Messages.Clear();
                }
            }
        }
        #endregion
    }
}
