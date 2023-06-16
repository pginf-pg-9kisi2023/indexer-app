using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Indexer.View
{
    internal class PreventAutoLoweringLengthConverter : IMultiValueConverter
    {
        private double CurrentMin;

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var length = (GridLength)values[0];
            var actualLength = (double)values[1];
            if (actualLength > CurrentMin)
            {
                CurrentMin = actualLength;
            }
            if (length == GridLength.Auto)
            {
                return CurrentMin;
            }
            return 0.0;
        }

        public object[] ConvertBack(
            object value, Type[] targetTypes, object parameter, CultureInfo culture
        )
        {
            throw new NotImplementedException();
        }
    }
}
