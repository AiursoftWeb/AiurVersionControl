using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace AiurVersionControl.SampleWPF.Components
{
    public class SameIdCheckerConverter : IValueConverter
    {
        /// <summary>
        /// If set to True, conversion is reversed: True will become Collapsed.
        /// </summary>
        public bool IsReversed { get; set; }
        
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var textBlock = parameter as TextBlock;
            string stringValue = System.Convert.ToString(value);
            bool val = textBlock != null && stringValue != null
                                         && stringValue.Equals(textBlock.Text);
            if (IsReversed) 
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