using ChatbotLib.DataObjects;
using ChatbotLib.Interfaces;
using FuzzySharp;
using FuzzySharp.Extractor;
using FuzzySharp.SimilarityRatio;
using FuzzySharp.SimilarityRatio.Scorer.StrategySensitive;

namespace ChatbotLib.Services
{
    public class AnswerFinderService : IAnswerFinderService
    {
        protected static string HierarchyFileName => "qa_main.json";
        protected IDataSavingService savingService;
        protected QuestionAnswerNode hierarchyHeadNode;
        protected QuestionAnswerNode currentContext;
        protected List<QuestionAnswerNode> allNodes = [];

        public AnswerFinderService(IDataSavingService savingService)
        {
            var loadedHierarchy = savingService.LoadDataAsJson<List<QuestionAnswerNode>>(HierarchyFileName).Result;
            if (loadedHierarchy is not null)
                hierarchyHeadNode = new() { ContextChildren = loadedHierarchy };
            else
                hierarchyHeadNode = new();
            currentContext = hierarchyHeadNode;
            AddNodeToAllNodes(hierarchyHeadNode);
            allNodes.Remove(hierarchyHeadNode);
            this.savingService = savingService;
        }
        protected void AddNodeToAllNodes(QuestionAnswerNode node)
        {
            allNodes.Add(node);
            foreach (QuestionAnswerNode subNode in node.ContextChildren)
                AddNodeToAllNodes(subNode);
        }

        public async Task<AnswerFinderResult> FindAnswerNode
            (string question, bool searchInContext = true, int minScoreForContext = 80, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            question = question.Trim().ToLower();
            // Первый поиск - в контексте (если набирается достаточный score - возвращаем его)
            var questions = currentContext.ContextChildren.Select(node => node.QuestionContextedNormalized);
            var result = await Task.Run(() => FindClosest(question, questions), token);
            if (result is not null && result.Score >= minScoreForContext)
                return ResultFindedActions(currentContext.ContextChildren.ToList()[result.Index], result.Score);

            // Второй поиск - вне контекста (возвращаем наибольший score)
            questions = allNodes.Select(node => node.QuestionNormalized);
            result = await Task.Run(() => FindClosest(question, questions), token);
            if (result is not null)
                return ResultFindedActions(allNodes[result.Index], result.Score);

            return new();

            // todo: сделать нормально
            AnswerFinderResult ResultFindedActions(QuestionAnswerNode node, int score)
            {
                AnswerFinderResult resultObject = new()
                {
                    FoundNode = node,
                    Score = score
                };
                ApplyContext(node);
                return resultObject;
            }
        }
        public void ApplyContext(QuestionAnswerNode node)
        {
            if (allNodes.Contains(node))
                currentContext = node;
        }
        public IEnumerable<string> GetContextQuestions()
            => currentContext.ContextChildren.Select(child => child.QuestionContexted);

        #region Методы сравнения строк
        protected static int CheckRation(string s1, string s2)
        {
            var result = Process.ExtractOne(s1, [s2], s => s, ScorerCache.Get<DefaultRatioScorer>());
            return result.Score;
        }
        protected static ExtractedResult<string> FindClosest(string toFind, IEnumerable<string> strings)
        {
            var result = Process.ExtractOne(toFind, strings, s => s, ScorerCache.Get<DefaultRatioScorer>());
            return result;
        }
        #endregion
    }
}
