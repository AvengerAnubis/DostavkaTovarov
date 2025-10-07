using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatbotLib
{
    public class QuestionAnswerNode
    {
        public string Question { get; set; } = string.Empty;
        public string QuestionNormalized => Question.Trim().ToLower();
        public string QuestionContexted { get; set; } = string.Empty;
        public string QuestionContextedNormalized => QuestionContexted.Trim().ToLower();

        public string Answer { get; set; } = string.Empty;
        public IEnumerable<QuestionAnswerNode> ContextChildren { get; set; } = [];
    }
}
