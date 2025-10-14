using ChatbotGui.QANEditor.Classes.Messages;
using ChatbotLib.Interfaces;
using ChatbotLib.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using System.IO;
using System.Text;
using System.Text.Json;

namespace ChatbotGui.QANEditor.ViewModels
{
    public partial class NodeEditorViewModel(
        IServiceProvider serviceProvider,
        IDataSavingService dataSavingService,
        IMessenger messeger,
        QuestionAnswerHierarchyViewModel hierarchy,
        QuestionAnswerNodeViewModel selectedNode) : ObservableObject
    {
        [ObservableProperty]
        protected QuestionAnswerHierarchyViewModel hierarchy = hierarchy;
        [ObservableProperty]
        protected QuestionAnswerNodeViewModel selectedNode = selectedNode;

        public static string MainSaveFilename => "qa_main.json";


        [RelayCommand]
        protected void SelectNode(QuestionAnswerNodeViewModel node)
        {
            messeger.Send<NodesChangedMessage>(new());
            SelectedNode = node;
        }

        [RelayCommand]
        protected void SaveNode()
            => messeger.Send<NodesChangedMessage>(new());

        // ContextChildren
        [RelayCommand]
        protected void AddContextNode()
        {
            QuestionAnswerNodeViewModel node = serviceProvider.GetRequiredService<QuestionAnswerNodeViewModel>();
            SelectedNode.ContextChildren.Add(node);
            messeger.Send<NodesChangedMessage>(new());
        }

        [RelayCommand]
        protected void RemoveContextNode(QuestionAnswerNodeViewModel node)
        {
            SelectedNode.ContextChildren.Remove(node);
            messeger.Send<NodesChangedMessage>(new());
        }

        [RelayCommand]
        protected void EditContextNode(QuestionAnswerNodeViewModel node)
            => SelectNode(node);

        // Сохранение в файл
        [RelayCommand]
        protected async Task SaveHierarchyToMain(CancellationToken token)
        {
            await dataSavingService.SaveDataAsJson(Hierarchy.HeadNode.ContextChildren.Select(vm => vm.GetModel()), MainSaveFilename, token);
        }
        [RelayCommand]
        protected async Task SaveHierarchyToUserFile(CancellationToken token)
        {
            string filename;
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "JSON format (.json)|*.json";
            dialog.DefaultExt = ".json";
            if (dialog.ShowDialog(serviceProvider.GetRequiredService<MainWindow>()) == true)
            {
                filename = dialog.FileName;

                using FileStream file = new(filename, FileMode.Create, FileAccess.Write);

                string json = JsonSerializer.Serialize(Hierarchy.HeadNode.ContextChildren.Select(vm => vm.GetModel()), DataSavingService.SerializerOptions);
                byte[] data = Encoding.UTF8.GetBytes(json);
                await file.WriteAsync(data, token);
            }
        }
    }
}
