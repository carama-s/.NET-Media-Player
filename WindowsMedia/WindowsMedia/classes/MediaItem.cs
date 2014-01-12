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
            return String.Format("{0:d2}:{1:d2}:{2:d2}", (int)duration.TotalHours, duration.Minutes, duration.Seconds);
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
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return Brushes.Black;

            Color color = (Color)value;
            return new SolidColorBrush(color);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    abstract public class MediaItem : INotifyPropertyChanged, ICloneable
    {
        public static String[] MusicExtensions = { ".mp3", ".flac", ".m4a" };
        public static String[] VideoExtensions = { ".mp4", ".mkv", ".avi", ".wmv" };
        public static String[] ImageExtensions = { ".jpg", ".jpeg", ".png", ".bmp" };

        public event PropertyChangedEventHandler PropertyChanged;

        private Color _messageColor;
        public Color MessageColor {
            get { return _messageColor; }
            set
            {
                _messageColor = value;
                OnPropertyChanged("MessageColor");
            }
        }
        public int Index { get; protected set; }
        public ClickStyle Type { get; protected set; }
        public string Artist { get; protected set; }
        public string Title { get; protected set; }
        public string Path { get; protected set; }
        public TimeSpan Duration { get; protected set; }
        public BitmapImage Image { get; protected set; }

        public void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        static public MediaItem Create(String path)
        {
            try
            {
                if (MusicExtensions.Contains(System.IO.Path.GetExtension(path)))
                    return new MusicTitle(path);
                else if (VideoExtensions.Contains(System.IO.Path.GetExtension(path)))
                    return new MovieFile(path);
                else if (ImageExtensions.Contains(System.IO.Path.GetExtension(path)))
                    return new ImageFile(path);
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public void SetIndex(int index)
        {
            this.Index = index;
        }

        abstract public Object Clone();
    }
}
