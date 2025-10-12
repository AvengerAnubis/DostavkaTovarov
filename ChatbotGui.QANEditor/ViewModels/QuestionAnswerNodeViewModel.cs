using ChatbotLib.DataObjects;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace ChatbotGui.QANEditor.ViewModels
{
    public partial class QuestionAnswerNodeViewModel : ObservableValidator
    {
        protected IServiceProvider serviceProvider;

        [ObservableProperty]
        [Required(AllowEmptyStrings = false)]
        protected string question = "Вопрос";
        [ObservableProperty]
        [Required(AllowEmptyStrings = false)]
        protected string questionContexted = "Вопрос с местоимениями";
        [ObservableProperty]
        [Required(AllowEmptyStrings = false)]
        protected string answer = "Ответ (каждое сообщение бота через знак \";\")";
        [ObservableProperty]
        protected ObservableCollection<QuestionAnswerNodeViewModel> contextChildren = [];

        public QuestionAnswerNodeViewModel(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }
        public QuestionAnswerNodeViewModel(IServiceProvider serviceProvider, QuestionAnswerNode node)
        {
            this.serviceProvider = serviceProvider;
            SetModel(node);
        }
        public void SetModel(QuestionAnswerNode node)
        {
            Question = node.Question;
            QuestionContexted = node.QuestionContexted;
            Answer = node.Answer;
            ContextChildren.Clear();
            foreach (var child in node.ContextChildren)
            {
                var nodeViewModel = serviceProvider.GetRequiredService<QuestionAnswerNodeViewModel>();
                nodeViewModel.SetModel(child);
                ContextChildren.Add(nodeViewModel);
            }
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
