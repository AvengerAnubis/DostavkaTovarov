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
        protected string questionContexted = "Вопрос с местоимениями";
        [ObservableProperty]
        protected string answer = "Ответ (каждое сообщение бота через знак \";\")";
        [ObservableProperty]
        protected ObservableCollection<QuestionAnswerNodeViewModel> contextChildren = [];
        [ObservableProperty]
        protected bool isNotRoot = true;
        [ObservableProperty]
        protected string keywords = "Ключевые слова (через пробел)";

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
            Keywords = node.Keywords;
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
                ContextChildren = [.. ContextChildren.Select(vm => vm.GetModel())],
                QuestionContexted = QuestionContexted,
                Keywords = Keywords
            };
            return node;
        }
    }
}
