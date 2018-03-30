using System;
using System.Globalization;
using System.Windows.Data;

namespace ZumtenSoft.Linq2ObsCollection.Samples.FolderSize
{
    [ValueConversion(typeof(String), typeof(String))]
    public class SizeConverter : IValueConverter
    {
        private static readonly string[] _scales = new[] {"B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB"};

        public SizeConverter()
        {
            Format = "{0:0.00} {1}";
        }

        public string Format { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int scale = 0;
            double size = ((long)value);
            while (size > 1000)
            {
                size /= 1024;
                scale++;
            }

            string format = (parameter as string) ?? Format;
            return String.Format(format, size, _scales[scale]);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
