using ChatbotGui.QANEditor.Views;
using System.Windows;

namespace ChatbotGui.QANEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly IServiceProvider serviceProvider;
        readonly NodeEditorView nodeEditorView;

        public MainWindow(IServiceProvider serviceProvider, NodeEditorView nodeEditorView)
        {
            InitializeComponent();
            this.serviceProvider = serviceProvider;
            this.nodeEditorView = nodeEditorView;

            this.mainFrame.Navigate(nodeEditorView);
        }
    }
}