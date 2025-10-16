using ChatbotLib.DataObjects;
using ChatbotLib.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Runtime.Intrinsics.Arm;
using System.Windows;

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
            RecommendedMessages.Clear();
            SendUserMessage(userMessage, true);

            List<AnswerFinderResult> result = [..await answerFinderService.FindAnswerNode(userMessage, true, 80, 5, token)];
            if (result.Count > 0)
            {
                /**
                MessageBox.Show(result
                    .Select(r => $"{r.FoundNode.Question} - {r.Score}")
                    .Aggregate((s1, s2) => $"{s1}\n{s2}"));
                **/

                if (result[0].Score >= 80)
                {
                    SendBotMessage($"Ваш вопрос: {result[0].FoundNode.Question}", true);
                    SendBotAnswers(result[0].FoundNode.Answer);
                    if (result[0].Score != 100)
                    {
                        SendBotMessage($"Если это не то, что вы искали, попробуйте переформулировать ваш вопрос " +
                            $"или поищите его из предложенных в списке справа.", false);
                        foreach (string relatedQuestion in result.Select(r => r.FoundNode.Question))
                            RecommendedMessages.Add(relatedQuestion);
                    }
                    else
                    {
                        answerFinderService.ApplyContext(result[0].FoundNode);
                        foreach (string contextQuestion in answerFinderService.GetContextQuestions())
                            RecommendedMessages.Add(contextQuestion);
                    }
                }
                else
                {
                    SendBotMessage("Я не понял ваш вопрос. Возможно, вы имели ввиду один из вопросов из списка справа.", true);
                    SendBotMessage($"Если это не то, что вы искали, попробуйте переформулировать ваш вопрос.", false);
                    foreach (string relatedQuestion in result.Select(r => r.FoundNode.Question))
                        RecommendedMessages.Add(relatedQuestion);
                }
            }
            else
            {
                SendBotMessage("Я не понял ваш вопрос. Попробуйте переформулировать ваш вопрос.", false);
                foreach (string contextQuestion in answerFinderService.GetContextQuestions())
                    RecommendedMessages.Add(contextQuestion);
            }
        }

        #region Методы отправки сообщений
        protected void SendBotAnswers(string answers)
        {
            string[] answersSplited = answers.Split(';');
            foreach (string answer in answersSplited)
            {
                SendBotMessage(answer, false);
            }
        }
        protected void SendUserMessage(string msg, bool isFirst)
        {
            ChatMessageViewModel message = serviceProvider.GetRequiredService<ChatMessageViewModel>();
            message.SetModel(new() { Author = "Вы", Message = msg });
            message.IsBotMessage = false;
            message.IsFirstMessage = isFirst;
            Messages.Add(message);
        }
        protected void SendBotMessage(string msg, bool isFirst)
        {
            ChatMessageViewModel botMessage = serviceProvider.GetRequiredService<ChatMessageViewModel>();
            botMessage.SetModel(new() { Author = "Bot", Message = msg });
            botMessage.IsBotMessage = true;
            botMessage.IsFirstMessage = isFirst;

            Messages.Add(botMessage);
        }
        #endregion
        [RelayCommand]
        protected async Task SendMessageFromRecommended(string message, CancellationToken token)
        {
            MessageToSend = message;
            await SendMessage(token);
        }
    }
}