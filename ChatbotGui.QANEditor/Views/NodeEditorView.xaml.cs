using ChatbotGui.QANEditor.ViewModels;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

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
            InitializeComponent();
            ViewModel = viewModel;
            ViewModel.SelectedNode = ViewModel.Hierarchy.HeadNode;
            DataContext = this;
        }
    }
}
