using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Indexer.View
{
    internal class MaxLengthConverter : MarkupExtension, IMultiValueConverter
    {
        public double? Length { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        public object Convert(
            object[] values, Type targetType, object parameter, CultureInfo culture
        )
        {
            var length = (GridLength)values[0];
            if (Length is not null && length == GridLength.Auto)
            {
                return Length;
            }
            var maxLength = (double)values[1];
            foreach (var value in values.Skip(2))
            {
                var toSubtract = (double)value;
                maxLength -= toSubtract;
            }
            return maxLength;
        }

        public object[] ConvertBack(
            object value, Type[] targetTypes, object parameter, CultureInfo culture
        )
        {
            throw new NotImplementedException();
        }
    }
}
