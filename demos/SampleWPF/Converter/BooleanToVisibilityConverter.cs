using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AiurVersionControl.SampleWPF.Converter
{
    /// <summary>
    /// Converts a Boolean into a Visibility.
    /// </summary>
    public class BooleanToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// If set to True, conversion is reversed: True will become Collapsed.
        /// </summary>
        public bool IsReversed { get; set; }
        
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var val = System.Convert.ToBoolean(value);
            if (this.IsReversed) 
            {
                val = !val;
            }

            return val ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}