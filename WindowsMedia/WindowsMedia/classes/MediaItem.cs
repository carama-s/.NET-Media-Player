using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace WindowsMedia.classes
{
    public class MediaItem
    {
        static public Uri DefaultImagePath = new Uri("../assets/defaultalbumart.png", UriKind.Relative);

        public string Title { get; private set; }
        public string Artist { get; private set; }
        public string Path { get; private set; }
        public TimeSpan Duration { get; private set; }
        public BitmapImage Image
        {
            get
            {
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
        }

        public MediaItem(MovieFile movie)
        {
            Title = movie.Title;
            Artist = "";
            Path = movie.Path;
            Duration = movie.Duration;
        }

        public MediaItem(ImageFile image)
        {
            Title = image.Title;
            Artist = "";
            Path = image.Path;
            Duration = image.Duration;
        }


    }
}
