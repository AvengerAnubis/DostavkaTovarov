using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatbotLib
{
    public class QuestionAnswerHierarchyNode
    {
        public string Question { get; set; } = string.Empty;
        public string Answer { get; set; } = string.Empty;
        public List<QuestionAnswerHierarchyNode> Children { get; } = [];
    }
}
