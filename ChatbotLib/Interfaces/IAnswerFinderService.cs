using ChatbotLib.DataObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatbotLib.Interfaces
{
    public interface IAnswerFinderService
    {
        Task<AnswerFinderResult> FindAnswerNode
            (string question, bool searchInContext = true, 
            int minScoreForContext = 80, CancellationToken token = default);
        void ApplyContext(QuestionAnswerNode node);

    }
}
