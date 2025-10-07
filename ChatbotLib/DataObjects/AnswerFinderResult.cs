using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatbotLib.DataObjects
{
    public class AnswerFinderResult
    {
        public QuestionAnswerNode FoundNode { get; set; } = new();
        public int Score { get; set; }
    }
}
