using ChatbotGui.Chat.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace ChatbotGui.Chat.Classes.TemplateSelectors
{
    public class ChatMessageTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? UserTemplate { get; set; }
        public DataTemplate? BotTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is ChatMessageViewModel msg)
                return msg.IsBotMessage ? BotTemplate! : UserTemplate!;
            return base.SelectTemplate(item, container);
        }
    }
}
