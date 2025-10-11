namespace ChatbotLib.DataObjects
{
    public class AnswerFinderResult
    {
        public QuestionAnswerNode? FoundNode { get; set; }
        public int Score { get; set; } = 0;
    }
}
