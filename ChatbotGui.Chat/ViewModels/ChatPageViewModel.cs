using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ChatbotGui.Chat.ViewModels
{
    public partial class ChatPageViewModel : ObservableObject
    {
        [ObservableProperty]
        protected string messageToSend = string.Empty;


        [RelayCommand]
        protected void SendMessage()
        {

        }
        [RelayCommand]
        protected void SendMessageFromRecommended()
        {

        }
    }
}