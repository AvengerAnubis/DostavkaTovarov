using ChatbotGui.Chat.ViewModels;
using System.Windows.Controls;

namespace ChatbotGui.Chat.Views
{
    /// <summary>
    /// Логика взаимодействия для ChatView.xaml
    /// </summary>
    public partial class ChatView : Page
    {
        public ChatViewModel ViewModel { get; set; }

        public ChatView(ChatViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();
        }
    }
}
