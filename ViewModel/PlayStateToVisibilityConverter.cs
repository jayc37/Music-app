using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using static Levitate.Core.Manager;

namespace Levitate.ViewModel
{
    public class PlayStateToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var state = (PlayerState)value;
            if (state == PlayerState.Playing)
                return Visibility.Hidden;
            else
                return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
