using System.Text.Json.Serialization;

namespace ChatbotLib.DataObjects
{
    public class QuestionAnswerNode
    {
        public string Question { get; set; } = string.Empty;
        [JsonIgnore] public string QuestionNormalized => Question.Trim().ToLower();
        public string QuestionContexted { get; set; } = string.Empty;
        [JsonIgnore] public string QuestionContextedNormalized => QuestionContexted.Trim().ToLower();

        public string Answer { get; set; } = string.Empty;
        public IEnumerable<QuestionAnswerNode> ContextChildren { get; set; } = [];
    }
}
