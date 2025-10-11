using ChatbotGui.QANEditor.Classes.Messages;
using ChatbotLib.DataObjects;
using ChatbotLib.Interfaces;
using ChatbotLib.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Win32;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;

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
            await dataSavingService.SaveDataAsJson(Hierarchy.HeadNode.GetModel(), "qa_main.json", token);
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

                if (File.Exists(filename))
                    File.Delete(filename);
                using FileStream file = File.OpenWrite(filename);

                string json = JsonSerializer.Serialize(Hierarchy.HeadNode.GetModel(), DataSavingService.SerializerOptions);
                byte[] data = Encoding.UTF8.GetBytes(json);
                await file.WriteAsync(data, token);
            }
        }
    }
}
