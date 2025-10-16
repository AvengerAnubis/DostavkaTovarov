using System.Text.Json.Serialization;

namespace ChatbotLib.DataObjects
{
    public class QuestionAnswerNode
    {
        public string Question { get; set; } = string.Empty;
        [JsonIgnore] public string QuestionNormalized => Question.Trim().ToLower();
        public string QuestionContexted { get; set; } = string.Empty;
        [JsonIgnore] public string QuestionContextedNormalized => QuestionContexted.Trim().ToLower();
        public string Keywords { get; set; } = string.Empty;
        public string KeywordsNormalized => Keywords.Trim().ToLower();
        public IEnumerable<string> KeywordsEnumerable => KeywordsNormalized.Split(' ');

        public string Answer { get; set; } = string.Empty;
        public List<QuestionAnswerNode> ContextChildren { get; set; } = [];
    }
}
