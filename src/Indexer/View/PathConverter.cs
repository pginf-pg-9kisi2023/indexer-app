using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;

namespace Indexer.View
{
    class PathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            String? strippedPath = Path.GetFileName(value.ToString());
            if (strippedPath == null)
            {
                return "null";
            }
            else
            {
                return strippedPath;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
