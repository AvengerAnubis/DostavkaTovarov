using ChatbotGui.QANEditor.Classes.Messages;
using ChatbotLib.DataObjects;
using ChatbotLib.Interfaces;
using CommunityToolkit.Mvvm;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatbotGui.QANEditor.ViewModels
{
    public partial class QuestionAnswerHierarchyViewModel
        : ObservableRecipient, IRecipient<NodeChangedMessage>
    {
        protected static string FileName => "editor_qa_hierarchy.json";
        protected readonly IDataSavingService dataSavingService;

        [ObservableProperty]
        protected QuestionAnswerNodeViewModel headNode;
        [ObservableProperty]
        protected ObservableCollection<QuestionAnswerNodeViewModel> headNodeCollection = [];

        [ObservableProperty]
        protected ObservableCollection<QuestionAnswerNodeViewModel> allNodes = [];

        public QuestionAnswerHierarchyViewModel(
            IDataSavingService dataSavingService,
            QuestionAnswerNodeViewModel headNodeViewModel)
        {
            this.dataSavingService = dataSavingService;
            HeadNode = headNodeViewModel;

            var headNode = dataSavingService.LoadDataAsJson<QuestionAnswerNode>(FileName).Result;
            if (headNode != null)
                HeadNode.SetModel(headNode);
            HeadNodeCollection.Add(HeadNode);

            AddNodeToAllNodes(HeadNode);
        }

        protected void AddNodeToAllNodes(QuestionAnswerNodeViewModel node)
        {
            AllNodes.Add(node);
            foreach (QuestionAnswerNodeViewModel subNode in node.ContextChildren)
                AddNodeToAllNodes(subNode);
            HeadNodeCollection.Clear();
            HeadNodeCollection.Add(HeadNode);
        }

        [RelayCommand]
        protected async Task SaveNodes(CancellationToken token)
        {
            await dataSavingService.SaveDataAsJson(HeadNode.GetModel(), FileName, token);
        }

        public void Receive(NodeChangedMessage message)
        {
            AllNodes.Clear();
            AddNodeToAllNodes(HeadNode);
        }
    }
}
