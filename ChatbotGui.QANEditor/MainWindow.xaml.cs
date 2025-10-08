using ChatbotGui.QANEditor.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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