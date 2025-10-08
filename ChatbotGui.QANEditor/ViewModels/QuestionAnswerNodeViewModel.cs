using ChatbotLib.DataObjects;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatbotGui.QANEditor.ViewModels
{
    public partial class QuestionAnswerNodeViewModel : ObservableValidator
    {
        [ObservableProperty]
        protected string question = string.Empty;
        [ObservableProperty]
        protected string questionContexted = string.Empty;
        [ObservableProperty]
        protected string answer = string.Empty;

        public ObservableCollection<QuestionAnswerNodeViewModel> ContextChildren { get; } = [];

        public QuestionAnswerNodeViewModel() { }
        public QuestionAnswerNodeViewModel(QuestionAnswerNode node)
        {
            SetModel(node);
        }
        public void SetModel(QuestionAnswerNode node)
        {
            Question = node.Question;
            QuestionContexted = node.QuestionContexted;
            Answer = node.Answer;
            ContextChildren.Clear();
            foreach (var child in node.ContextChildren)
                ContextChildren.Add(new(child));
        }
        public QuestionAnswerNode GetModel()
        {
            QuestionAnswerNode node = new()
            {
                Question = Question,
                Answer = Answer,
                ContextChildren = ContextChildren.Select(vm => vm.GetModel()),
                QuestionContexted = QuestionContexted
            };
            return node;
        }
    }
}
