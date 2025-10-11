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
        : ObservableRecipient, IRecipient<NodesChangedMessage>, IDisposable
    {
        protected static string FileName => "editor_qa_hierarchy.json";
        protected readonly IDataSavingService dataSavingService;
        protected readonly IMessenger messenger;

        [ObservableProperty]
        protected QuestionAnswerNodeViewModel headNode;
        [ObservableProperty]
        protected ObservableCollection<QuestionAnswerNodeViewModel> headNodeCollection = [];

        [ObservableProperty]
        protected ObservableCollection<QuestionAnswerNodeViewModel> allNodes = [];

        public QuestionAnswerHierarchyViewModel(
            IDataSavingService dataSavingService,
            IMessenger messenger,
            QuestionAnswerNodeViewModel headNodeViewModel)
        {
            this.dataSavingService = dataSavingService;
            this.messenger = messenger;
            this.messenger.Register(this);
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

        protected async Task SaveNodes(CancellationToken token = default)
        {
            await dataSavingService.SaveDataAsJson(HeadNode.GetModel(), FileName, token);
        }

        public void Receive(NodesChangedMessage message)
        {
            Task.Run(async () => await SaveNodes());
        }

        #region Disposing
        protected bool isDisposed = false;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                isDisposed = true;
                if (disposing)
                {
                    messenger.Unregister<NodesChangedMessage>(this);
                }
            }
        }
        #endregion
    }
}
