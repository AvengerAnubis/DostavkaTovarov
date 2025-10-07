using System.Text;
using System.Text.Json;

namespace ChatbotLib.Tests
{
    [Trait("Category", "Modules")]
    public class ChatServiceTests
    {
        private string FilePath => Path.Combine(Directory.GetCurrentDirectory(), "chathistory.json");

        private static DataSavingService CreateService() => new();

        [Fact]
        public void ChatService_SendMessage_AddsMessagesCorrectly()
        {
            using var chat = new ChatService(CreateService());
            chat.SendMessage("User", "Hello");
            chat.SendMessage("Bot", "Hi there");

            var messages = chat.Messages.ToList();

            Assert.Equal(2, messages.Count);
            Assert.Equal("User", messages[0].Author);
            Assert.Equal("Hello", messages[0].Message);
            Assert.Equal("Bot", messages[1].Author);
            Assert.Equal("Hi there", messages[1].Message);
        }

        [Fact]
        public void ChatService_SendMessage_LimitExceeded_RemovesOldestMessage()
        {
            using var chat = new ChatService(CreateService(), limit: 3);

            chat.SendMessage("A", "1");
            chat.SendMessage("B", "2");
            chat.SendMessage("C", "3");
            chat.SendMessage("D", "4"); // превышаем лимит

            var messages = chat.Messages.ToList();
            Assert.Equal(3, messages.Count);
            Assert.DoesNotContain(messages, m => m.Message == "1");
            Assert.Equal("2", messages[0].Message);
            Assert.Equal("4", messages[^1].Message);
        }

        [Fact]
        public async Task ChatService_SaveToJson_CreatesValidJsonFile()
        {
            using var savingService = CreateService();
            using var chat = new ChatService(savingService);

            chat.SendMessage("User", "Hello");
            chat.SendMessage("Bot", "Hi!");

            await chat.SaveToJson();

            Assert.True(File.Exists(FilePath));

            string json = await File.ReadAllTextAsync(FilePath);
            Assert.Contains("User", json);
            Assert.Contains("Hello", json);

            File.Delete(FilePath);
        }

        [Fact]
        public async Task ChatService_LoadFromJson_LoadsSavedMessages()
        {
            using var savingService = CreateService();
            using var chat = new ChatService(savingService);

            // Создаём тестовый файл
            var messages = new Queue<ChatMessage>(new[]
            {
                new ChatMessage { Author = "Alice", Message = "Hi" },
                new ChatMessage { Author = "Bob", Message = "Hello" }
            });

            await savingService.SaveDataAsJson(messages, "chathistory.json");

            await chat.LoadFromJson();

            var result = chat.Messages.ToList();
            Assert.Equal(2, result.Count);
            Assert.Equal("Alice", result[0].Author);
            Assert.Equal("Bob", result[1].Author);

            File.Delete(FilePath);
        }

        [Fact]
        public async Task ChatService_LoadFromJson_InvalidJson_ClearsMessages()
        {
            using var savingService = CreateService();
            using var chat = new ChatService(savingService);

            await File.WriteAllTextAsync(FilePath, "{invalid_json:true");

            chat.SendMessage("Old", "Message"); // добавим старое сообщение
            await chat.LoadFromJson();

            Assert.Empty(chat.Messages);

            File.Delete(FilePath);
        }

        [Fact]
        public async Task ChatService_LoadFromJson_FileNotFound_ClearsMessages()
        {
            using var savingService = CreateService();
            using var chat = new ChatService(savingService);

            chat.SendMessage("Existing", "Data");
            await chat.LoadFromJson(); // файла нет

            Assert.Empty(chat.Messages);
        }

        [Fact]
        public async Task ChatService_SaveToJson_RespectsCancellationToken()
        {
            using var savingService = CreateService();
            using var chat = new ChatService(savingService);

            chat.SendMessage("User", "Hello");
            using var cts = new CancellationTokenSource();

            cts.Cancel();

            await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            {
                await chat.SaveToJson(cts.Token);
            });
        }

        [Fact]
        public async Task ChatService_LoadFromJson_RespectsCancellationToken()
        {
            using var savingService = CreateService();
            using var chat = new ChatService(savingService);

            using var cts = new CancellationTokenSource();
            cts.Cancel();

            await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            {
                await chat.LoadFromJson(cts.Token);
            });
        }

        [Fact]
        public void ChatService_Dispose_ClearsMessagesAndCancelsToken()
        {
            var savingService = CreateService();
            var chat = new ChatService(savingService);

            chat.SendMessage("User", "Hello");
            chat.Dispose();

            Assert.Empty(chat.Messages);

            // Второй вызов Dispose не должен кидать исключение
            chat.Dispose();
        }
    }
}
