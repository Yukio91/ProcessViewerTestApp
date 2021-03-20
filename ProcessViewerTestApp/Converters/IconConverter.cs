using ProcessViewerTestApp.Extensions;
using System;
using System.Windows.Data;

namespace ProcessViewerTestApp.Converters
{
    public class IconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is System.Drawing.Icon)
            {
                var icon = value as System.Drawing.Icon;
                return icon.ToImageSource();
            }

            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }
}
