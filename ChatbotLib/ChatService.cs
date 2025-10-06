using System.IO.Pipelines;
using System.Reflection;
using System.Text.Json;

namespace ChatbotLib
{
    public class ChatService : IDisposable
    {
        public List<ChatMessage> Messages { get; protected set; } = [];

        #region Отправка сообщений
        public void SendMessage(ChatMessage message) => Messages.Add(message);
        public void SendMessage(string author, string message) 
            => SendMessage(new() { Author = author, Message = message });
        #endregion
        #region Serialization/Deserialization
        protected JsonSerializerOptions options = new()
        {
            WriteIndented = false,
            IncludeFields = false,
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        };
        protected CancellationTokenSource sharedCts = new();
        public async Task SaveToJson(CancellationToken token = default)
        {
            var registerToken = token.Register(() => sharedCts.Cancel());

            using FileStream file = File.OpenWrite(Assembly.GetExecutingAssembly().Location + @"\chat.json");

            await JsonSerializer.SerializeAsync(file, Messages, options, sharedCts.Token);

            registerToken.Unregister();
        }
        public async Task LoadFromJson(CancellationToken token = default)
        {
            var registerToken = token.Register(() => sharedCts.Cancel());

            try
            {
                using FileStream file = File.OpenRead(Assembly.GetExecutingAssembly().Location + @"\chat.json");
                List<ChatMessage>? messages = await JsonSerializer.DeserializeAsync<List<ChatMessage>>(file, options, sharedCts.Token);
                if (messages is not null) 
                    Messages = messages;
            }
            catch (Exception ex) when (
                ex is UnauthorizedAccessException ||
                ex is FileNotFoundException ||
                ex is JsonException
            )
            {
                Messages.Clear();
            }


            registerToken.Unregister();
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
                }
            }
        }
        #endregion
    }
}
