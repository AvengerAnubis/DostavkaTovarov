using ChatbotGui.Chat.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
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
