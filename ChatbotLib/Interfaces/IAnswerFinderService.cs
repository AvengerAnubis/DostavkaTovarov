using ChatbotLib.DataObjects;

namespace ChatbotLib.Interfaces
{
    public interface IAnswerFinderService
    {
        Task<IEnumerable<AnswerFinderResult>> FindAnswerNode
            (string question, bool searchInContext = true,
            int minScoreForContext = 80, int limit = 5, CancellationToken token = default);
        void ApplyContext(QuestionAnswerNode node);
        IEnumerable<string> GetContextQuestions();
    }
}
