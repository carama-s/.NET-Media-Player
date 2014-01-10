using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace WindowsMedia.classes
{
    [ValueConversion(typeof(String), typeof(String))]
    class BiblioNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Path.GetFileNameWithoutExtension((String)value);
        }

        public object ConvertBack(object value, Type TargetType, object parameter, CultureInfo culture)
        {
            String str = (String)value;
            return str;
        }
    }
}
