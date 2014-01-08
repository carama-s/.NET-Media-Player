using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WindowsMedia.classes
{
    [ValueConversion(typeof(TimeSpan), typeof(String))]
    public class DurationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            TimeSpan duration = (TimeSpan)value;
            return String.Format("{0:d2}:{1:d2}:{2:d2}", duration.Hours, duration.Minutes, duration.Seconds);
        }

        public object ConvertBack(object value, Type TargetType, object parameter, CultureInfo culture)
        {
            String str = (String)value;
            TimeSpan result;
            if (TimeSpan.TryParse(str, out result))
                return result;
            return new TimeSpan();
        }
    }

    [ValueConversion(typeof(Int32), typeof(String))]
    public class NbElementsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var nb = (Int32)value;
            return String.Format("{0:d2} {1:d2}", nb.ToString(), "éléments");
        }

        public object ConvertBack(object value, Type TargetType, object parameter, CultureInfo culture)
        {
            String str = (String)value;
            String[] result = str.Split(new string[] { " " }, StringSplitOptions.None);

            return result[0];
        }
    }

    public class ColorToBrushConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return Brushes.Black;

            Color color = (Color)value;
            Console.Out.WriteLine("CONVERT");
            return new SolidColorBrush(color);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    abstract public class MediaItem : INotifyPropertyChanged, ICloneable
    {
        static public Uri DefaultImagePath = new Uri("../assets/defaultalbumart.png", UriKind.Relative);
        
        public event PropertyChangedEventHandler PropertyChanged;

        private Color _messageColor;
        public Color MessageColor {
            get { return _messageColor; }
            set
            {
                _messageColor = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("MessageColor"));
            }
        }
        public ClickStyle Type { get; protected set; }
        public string Artist { get; protected set; }
        public string Title { get; protected set; }
        public string Path { get; protected set; }
        public TimeSpan Duration { get; protected set; }
        public BitmapImage Image
        {
            get
            {
                return GetImage();
            }

            private set { }
        }

        abstract protected BitmapImage GetImage();
        abstract public Object Clone();
    }
}
