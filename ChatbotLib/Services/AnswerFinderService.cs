using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatbotLib.DataObjects;
using ChatbotLib.Interfaces;
using FuzzySharp;
using FuzzySharp.Extractor;
using FuzzySharp.SimilarityRatio;
using FuzzySharp.SimilarityRatio.Scorer.StrategySensitive;

namespace ChatbotLib.Services
{
    public class AnswerFinderService : IDisposable, IAnswerFinderService
    {
        protected string HierarchyFileName => "qa_hierarchy.json";
        protected IDataSavingService savingService;
        protected QuestionAnswerNode hierarchyHeadNode;
        protected QuestionAnswerNode currentContext;
        protected List<QuestionAnswerNode> allNodes = [];
        protected CancellationTokenSource sharedCts = new();

        public AnswerFinderService(IDataSavingService savingService)
        {
            var loadedHierarchy = savingService.LoadDataAsJson<QuestionAnswerNode>(HierarchyFileName).Result;
            if (loadedHierarchy is not null)
                hierarchyHeadNode = loadedHierarchy;
            else
                hierarchyHeadNode = new();
            currentContext = new() { ContextChildren = [hierarchyHeadNode] };
            AddNodeToAllNodes(hierarchyHeadNode);
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
            var registerToken = token.Register(() => sharedCts.Cancel());

            try
            {
                question = question.Trim().ToLower();
                // Первый поиск - в контексте (если набирается достаточный score - возвращаем его)
                var questions = currentContext.ContextChildren.Select(node => node.QuestionContextedNormalized);
                var result = await Task.Run(() => FindClosest(question, questions), token);
                if (result.Score >= minScoreForContext)
                    return new() { FoundNode = allNodes[result.Index], Score = result.Score };

                // Второй поиск - вне контекста (возвращаем наибольший score)
                questions = allNodes.Select(node => node.QuestionNormalized);
                result = await Task.Run(() => FindClosest(question, questions), token);
                return new() { FoundNode = allNodes[result.Index], Score = result.Score };
            }
            finally
            {
                registerToken.Unregister();
            }
        }
        public void ApplyContext(QuestionAnswerNode node)
        {
            if (allNodes.Contains(node))
                currentContext = node;
        }

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

        #region Disposing
        protected bool isDisposed = false;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                isDisposed = true;
                if (disposing)
                {
                    sharedCts.Cancel();
                    sharedCts.Dispose();
                }
            }
        }
        #endregion
    }
}
