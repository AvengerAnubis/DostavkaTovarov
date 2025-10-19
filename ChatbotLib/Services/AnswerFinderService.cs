using ChatbotLib.DataObjects;
using ChatbotLib.Interfaces;
using Raffinert.FuzzySharp;
using Raffinert.FuzzySharp.Extractor;
using Raffinert.FuzzySharp.SimilarityRatio;
using Raffinert.FuzzySharp.SimilarityRatio.Scorer.Composite;
using Raffinert.FuzzySharp.SimilarityRatio.Scorer.StrategySensitive;
using System.Xml.Linq;

namespace ChatbotLib.Services
{
    public class AnswerFinderService : IAnswerFinderService
    {
        protected static string HierarchyFileName => "qa_main.json";
        protected IDataSavingService savingService;
        protected QuestionAnswerNode hierarchyHeadNode;
        public QuestionAnswerNode HierarchyHeadNode => hierarchyHeadNode;
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

        public void ApplyContext(QuestionAnswerNode node)
        {
            if (allNodes.Contains(node))
                currentContext = node;
        }
        public IEnumerable<string> GetContextQuestions()
            => currentContext.ContextChildren.Select(child => child.Question);

        public async Task<IEnumerable<AnswerFinderResult>> FindAnswerNode
            (string question, bool searchInContext = true, int minScoreForContext = 80, 
            int limit = 5, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            question = question.Trim().ToLower();
            // Первый поиск - в контексте (если набирается достаточный score - возвращаем его)
            List<string> questions = [.. currentContext.ContextChildren.Select(node => node.QuestionContextedNormalized)];
            var result = await Task.Run(() => FindBest(question, questions), token);
            if (result is not null && result.Score >= minScoreForContext)
            {
                QuestionAnswerNode answerNode = currentContext.ContextChildren[result.Index];
                return [new() { FoundNode = answerNode, Score = result.Score}];
            }
            else
            {
                // Второй поиск - вне контекста:
                questions = [.. allNodes.Select(node => node.QuestionNormalized)];
                //  - фильтруем по ключевым словам
                var indexes = FilterByKeywords(question, questions);
                List<QuestionAnswerNode> filteredNodes = [];
                foreach (var index in indexes)
                    filteredNodes.Add(allNodes[index]);
                //  - возвращаем {limit} количество нод (топ по score)
                var results = await Task.Run(() => FindTop(question, filteredNodes.Select(n => n.QuestionNormalized), limit), token);
                return results.Select(r => new AnswerFinderResult() 
                { 
                    FoundNode = filteredNodes[r.Index], Score = r.Score 
                });
            }
            
        }

        #region Методы поиска
        protected static IEnumerable<int> FilterByKeywords(string s, IEnumerable<string> strings)
        {
            var result = Process.ExtractAll(s, strings, scorer: ScorerCache.Get<PartialTokenSetScorer>(), cutoff: 60);
            return result.Select(r => r.Index);
        }
        #endregion
        #region Методы сравнения строк
        protected static int CheckRation(string s1, string s2)
            => Fuzz.PartialRatio(s1, s2);
        protected static ExtractedResult<string> FindBest(string toFind, IEnumerable<string> strings)
            => Process.ExtractOne(toFind, strings, scorer: ScorerCache.Get<WeightedRatioScorer>());
        protected static IEnumerable<ExtractedResult<string>> FindTop(string toFind, IEnumerable<string> strings, int limit)
            => Process.ExtractTop(toFind, strings, scorer: ScorerCache.Get<WeightedRatioScorer>(), limit: limit);
        #endregion
    }
}
