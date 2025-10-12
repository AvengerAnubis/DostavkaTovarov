using ChatbotLib.DataObjects;
using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ChatbotGui.Chat.ViewModels
{
    public partial class ChatMessageViewModel : ObservableValidator
    {
        protected IServiceProvider serviceProvider;

        [ObservableProperty]
        [Required(AllowEmptyStrings = false)]
        protected string author = "noname";
        [ObservableProperty]
        [Required(AllowEmptyStrings = false)]
        protected string message = "Hello, World!";

        [ObservableProperty]
        protected bool isBotMessage = false;
        [ObservableProperty]
        protected bool isFirstMessage = true;

        public ChatMessageViewModel(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }
        public ChatMessageViewModel(IServiceProvider serviceProvider, ChatMessage msg)
        {
            this.serviceProvider = serviceProvider;
            SetModel(msg);
        }
        public void SetModel(ChatMessage msg)
        {
            Author = msg.Author;
            Message = msg.Message;
        }
        public ChatMessage GetModel()
        {
            ChatMessage msg = new()
            {
                Author = Author,
                Message = Message
            };
            return msg;
        }
    }
}
