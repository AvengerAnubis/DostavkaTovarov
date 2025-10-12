using ChatbotGui.QANEditor.ViewModels;
using System.Windows.Controls;

namespace ChatbotGui.QANEditor.Views
{
    /// <summary>
    /// Логика взаимодействия для NodeEditorView.xaml
    /// </summary>
    public partial class NodeEditorView : Page
    {
        public NodeEditorViewModel ViewModel { get; }

        public NodeEditorView(NodeEditorViewModel viewModel)
        {
            ViewModel = viewModel;
            ViewModel.SelectedNode = ViewModel.Hierarchy.HeadNode;
            DataContext = this;
            InitializeComponent();
        }
    }
}
