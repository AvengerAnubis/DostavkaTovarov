using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace ChatbotGui.Chat.Classes.ValueConverters
{
    public class VisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType == typeof(Visibility))
                if (value is bool valueBool)
                    return (valueBool) ? Visibility.Visible : Visibility.Collapsed;
            return null!;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType == typeof(bool))
                if (value is Visibility valueVisibility)
                    return (valueVisibility == Visibility.Visible) ? true : false;
            return null!;
        }
    }
}
