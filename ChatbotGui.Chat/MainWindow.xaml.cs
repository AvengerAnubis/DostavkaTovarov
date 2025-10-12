using ChatbotGui.Chat.Views;
using Wpf.Ui.Controls;

namespace ChatbotGui.Chat
{
    /// <summary>
    /// Логика взаимодействия для Window1.xaml
    /// </summary>
    public partial class MainWindow : FluentWindow
    {
        readonly IServiceProvider serviceProvider;
        readonly ChatView nodeEditorView;

        public MainWindow(IServiceProvider serviceProvider, ChatView nodeEditorView)
        {
            InitializeComponent();
            this.serviceProvider = serviceProvider;
            this.nodeEditorView = nodeEditorView;

            this.mainFrame.Navigate(nodeEditorView);
        }
    }
}
