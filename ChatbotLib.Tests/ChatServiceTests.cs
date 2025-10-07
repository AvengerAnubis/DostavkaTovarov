using System.Text;
using System.Text.Json;
using ChatbotLib.DataObjects;
using ChatbotLib.Interfaces;
using ChatbotLib.Services;
using Moq;

namespace ChatbotLib.Tests
{
    [Trait("Category", "Modules")]
    public class ChatServiceTests
    {
        [Fact]
        public void ChatService_SendMessage_AddsMessagesCorrectly()
        {
            var mockSaver = new Mock<IDataSavingService>();
            using var chat = new ChatService(mockSaver.Object);

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
            var mockSaver = new Mock<IDataSavingService>();
            using var chat = new ChatService(mockSaver.Object, limit: 3);

            chat.SendMessage("A", "1");
            chat.SendMessage("B", "2");
            chat.SendMessage("C", "3");
            chat.SendMessage("D", "4");

            var messages = chat.Messages.ToList();

            Assert.Equal(3, messages.Count);
            Assert.DoesNotContain(messages, m => m.Message == "1");
            Assert.Equal("2", messages[0].Message);
            Assert.Equal("4", messages[^1].Message);
        }

        [Fact]
        public async Task ChatService_SaveToJson_CallsDataSavingService()
        {
            var mockSaver = new Mock<IDataSavingService>();
            mockSaver
                .Setup(s => s.SaveDataAsJson(It.IsAny<object>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            using var chat = new ChatService(mockSaver.Object);

            chat.SendMessage("User", "Hello");
            chat.SendMessage("Bot", "Hi!");

            await chat.SaveChatHistory();

            mockSaver.Verify(s => s.SaveDataAsJson(
                It.IsAny<object>(),
                "chathistory.json",
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task ChatService_LoadFromJson_CallsDataSavingService_AndReplacesMessages()
        {
            var mockSaver = new Mock<IDataSavingService>();
            var fakeMessages = new Queue<ChatMessage>(new[]
            {
                new ChatMessage { Author = "Alice", Message = "Hi" },
                new ChatMessage { Author = "Bob", Message = "Hello" }
            });

            mockSaver
                .Setup(s => s.LoadDataAsJson<Queue<ChatMessage>>("chathistory.json", It.IsAny<CancellationToken>()))
                .ReturnsAsync(fakeMessages);

            using var chat = new ChatService(mockSaver.Object);

            await chat.LoadChatHistory();

            var result = chat.Messages.ToList();
            Assert.Equal(2, result.Count);
            Assert.Equal("Alice", result[0].Author);
            Assert.Equal("Bob", result[1].Author);
        }

        [Fact]
        public async Task ChatService_LoadFromJson_ReturnsNull_ClearsMessages()
        {
            var mockSaver = new Mock<IDataSavingService>();
            mockSaver
                .Setup(s => s.LoadDataAsJson<Queue<ChatMessage>>("chathistory.json", It.IsAny<CancellationToken>()))
                .ReturnsAsync((Queue<ChatMessage>?)null);

            using var chat = new ChatService(mockSaver.Object);

            chat.SendMessage("Old", "Message");
            await chat.LoadChatHistory();

            Assert.Empty(chat.Messages);
        }

        [Fact]
        public async Task ChatService_LoadFromJson_ThrowsException_ClearsMessages()
        {
            var mockSaver = new Mock<IDataSavingService>();
            mockSaver
                .Setup(s => s.LoadDataAsJson<Queue<ChatMessage>>("chathistory.json", It.IsAny<CancellationToken>()))
                .ThrowsAsync(new JsonException());

            using var chat = new ChatService(mockSaver.Object);

            chat.SendMessage("Before", "Test");
            await chat.LoadChatHistory();

            Assert.Empty(chat.Messages);
        }

        [Fact]
        public async Task ChatService_SaveToJson_RespectsCancellationToken()
        {
            var mockSaver = new Mock<IDataSavingService>();
            mockSaver
                .Setup(s => s.SaveDataAsJson(It.IsAny<object>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new OperationCanceledException());

            using var chat = new ChatService(mockSaver.Object);
            chat.SendMessage("User", "Hello");

            using var cts = new CancellationTokenSource();
            cts.Cancel();

            await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            {
                await chat.SaveChatHistory(cts.Token);
            });
        }

        [Fact]
        public async Task ChatService_LoadFromJson_RespectsCancellationToken()
        {
            var mockSaver = new Mock<IDataSavingService>();
            mockSaver
                .Setup(s => s.LoadDataAsJson<Queue<ChatMessage>>("chathistory.json", It.IsAny<CancellationToken>()))
                .ThrowsAsync(new OperationCanceledException());

            using var chat = new ChatService(mockSaver.Object);
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            {
                await chat.LoadChatHistory(cts.Token);
            });
        }

        [Fact]
        public void ChatService_Dispose_ClearsMessagesAndDoesNotThrow()
        {
            var mockSaver = new Mock<IDataSavingService>();
            var chat = new ChatService(mockSaver.Object);

            chat.SendMessage("User", "Hello");
            chat.Dispose();

            Assert.Empty(chat.Messages);
            chat.Dispose(); // повторный вызов безопасен
        }
    }
}
