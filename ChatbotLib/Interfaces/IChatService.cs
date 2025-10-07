using ChatbotLib.DataObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatbotLib.Interfaces
{
    public interface IChatService
    {
        void SendMessage(ChatMessage message);
        void SendMessage(string author, string message);
        Task SaveChatHistory(CancellationToken token = default);
        Task LoadChatHistory(CancellationToken token = default);
    }
}
