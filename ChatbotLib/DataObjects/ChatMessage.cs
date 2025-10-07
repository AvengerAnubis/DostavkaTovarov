using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatbotLib.DataObjects
{
    public class ChatMessage
    {
        public string Author { get; set; } = "noname";
        public string Message { get; set; } = string.Empty;
    }
}
