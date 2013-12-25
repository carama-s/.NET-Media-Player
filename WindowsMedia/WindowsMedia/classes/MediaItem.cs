using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;

namespace WindowsMedia.classes
{
    [ValueConversion(typeof(Object), typeof(String))]
    public class MediaItemTitleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var obj = (MediaItem)value;
            if (obj != null)
                return obj.Title;
            else
                return "";
        }

        public object ConvertBack(object value, Type TargetType, object parameter, CultureInfo culture)
        {
            return (Object)value;
        }
    }

    [ValueConversion(typeof(Object), typeof(String))]
    public class MediaItemArtistConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var obj = (MediaItem)value;
            if (obj != null)
                return obj.Artist;
            else
                return "";
        }

        public object ConvertBack(object value, Type TargetType, object parameter, CultureInfo culture)
        {
            return (Object)value;
        }
    }

    [ValueConversion(typeof(Object), typeof(BitmapImage))]
    public class MediaItemImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var obj = (MediaItem)value;
            if (obj != null)
                return obj.Image;
            else
                return null;
        }

        public object ConvertBack(object value, Type TargetType, object parameter, CultureInfo culture)
        {
            return (Object)value;
        }
    }

    public class MediaItem
    {
        static public Uri DefaultImagePath = new Uri("../assets/defaultalbumart.png", UriKind.Relative);
        public string Title { get; private set; }
        public string Artist { get; private set; }
        public string Path { get; private set; }
        public TimeSpan Duration { get; private set; }
        private bool IsImage { get; set; }
        public BitmapImage Image
        {
            get
            {
                if (IsImage)
                    return new BitmapImage(new Uri(Path, UriKind.Absolute));
                TagLib.File tags = null;
                try
                {
                    tags = TagLib.File.Create(Path);
                }
                catch (FileNotFoundException)
                {
                    return new BitmapImage(DefaultImagePath);
                }
                if (tags.Tag.Pictures.Length > 0)
                {
                    var img = new BitmapImage();
                    img.BeginInit();
                    img.StreamSource = new MemoryStream(tags.Tag.Pictures[0].Data.Data);
                    img.EndInit();
                    return img;
                }
                else
                    return new BitmapImage(DefaultImagePath);
            }

            private set { }
        }

        public MediaItem(MusicTitle music)
        {
            Title = music.Title;
            Artist = music.Artist;
            Path = music.Path;
            Duration = music.Duration;
            IsImage = false;
        }

        public MediaItem(MovieFile movie)
        {
            Title = movie.Title;
            Artist = "";
            Path = movie.Path;
            Duration = movie.Duration;
            IsImage = false;
        }

        public MediaItem(ImageFile image)
        {
            Title = image.Title;
            Artist = "";
            Path = image.Path;
            Duration = image.Duration;
            IsImage = true;
        }
    }
}
