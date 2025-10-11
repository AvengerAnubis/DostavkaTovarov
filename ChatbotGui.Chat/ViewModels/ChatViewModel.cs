using ChatbotLib.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;

namespace ChatbotGui.Chat.ViewModels
{
    public partial class ChatViewModel(IServiceProvider serviceProvider, IAnswerFinderService answerFinderService) : ObservableObject
    {
        [ObservableProperty]
        protected string messageToSend = string.Empty;
        [ObservableProperty]
        protected ObservableCollection<ChatMessageViewModel> messages = [];
        [ObservableProperty]
        protected ObservableCollection<string> recommendedMessages = [.. answerFinderService.GetContextQuestions()];

        [RelayCommand]
        protected async Task SendMessage(CancellationToken token)
        {
            string userMessage = MessageToSend;
            MessageToSend = string.Empty;
            ChatMessageViewModel message = serviceProvider.GetRequiredService<ChatMessageViewModel>();
            message.SetModel(new() { Author = "Вы", Message = userMessage });
            Messages.Add(message);

            var result = await answerFinderService.FindAnswerNode(userMessage, true, 80, token);
            if (result.FoundNode is not null)
            {
                // !!! todo: добавить при определенный баллах варианты ответа бота:
                //           "Возможно, вы имели ввиду ..."
                //           "Не понял вас ..."
                string[] answerSplited = result.FoundNode.Answer.Split(';');
                bool isFirst = true;
                foreach (string answer in answerSplited)
                {
                    ChatMessageViewModel botMessage = serviceProvider.GetRequiredService<ChatMessageViewModel>();
                    botMessage.SetModel(new() { Author = "Bot", Message = answer });
                    botMessage.IsBotMessage = true;
                    botMessage.IsFirstMessage = isFirst;
                    isFirst = false;
                    Messages.Add(botMessage);
                }

                RecommendedMessages.Clear();
                foreach (string contextQuestion in answerFinderService.GetContextQuestions())
                    RecommendedMessages.Add(contextQuestion);
            }
            else
            {
                // todo: уведомить пользователя о том, что дерево вопросов не заполнено
            }
            // todo: очистить код по методам, как будет время
        }
        [RelayCommand]
        protected async Task SendMessageFromRecommended(string message, CancellationToken token)
        {
            MessageToSend = message;
            await SendMessage(token);
        }
    }
}