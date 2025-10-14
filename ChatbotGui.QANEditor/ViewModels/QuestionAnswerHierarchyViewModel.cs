using ChatbotGui.QANEditor.Classes.Messages;
using ChatbotLib.DataObjects;
using ChatbotLib.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;

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
            IServiceProvider serviceProvider,
            IDataSavingService dataSavingService,
            IMessenger messenger,
            QuestionAnswerNodeViewModel headNodeViewModel)
        {
            this.dataSavingService = dataSavingService;
            this.messenger = messenger;
            this.messenger.Register(this);
            HeadNode = headNodeViewModel;
            HeadNode.Question = "Root";
            HeadNode.QuestionContexted = string.Empty;
            HeadNode.Answer = string.Empty;
            HeadNode.IsNotRoot = false;

            var headNode = dataSavingService.LoadDataAsJson<List<QuestionAnswerNode>>(FileName).Result;
            if (headNode != null)
            {
                foreach (var node in headNode)
                {
                    var nodeViewModel = serviceProvider.GetRequiredService<QuestionAnswerNodeViewModel>();
                    nodeViewModel.SetModel(node);
                    HeadNode.ContextChildren.Add(nodeViewModel);
                }
            }
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
            await dataSavingService.SaveDataAsJson(HeadNode.ContextChildren.Select(vm => vm.GetModel()), FileName, token);
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
