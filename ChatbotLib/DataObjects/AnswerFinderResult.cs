namespace ChatbotLib.DataObjects
{
    public class AnswerFinderResult
    {
        public QuestionAnswerNode FoundNode { get; set; } = new();
        public int Score { get; set; }
    }
}
