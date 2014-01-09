using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WindowsMedia.classes
{
    public class MusicTitle : MediaItem
    {
        static public Uri DefaultImagePath = new Uri("../assets/defaultalbumart.png", UriKind.Relative);
        public String Album { get; private set; }
        public String Genre { get; private set; }
        public uint Year { get; private set; }
        public uint TrackNumber { get; private set; }
        public String Composer { get; private set; }

        protected override BitmapImage GetImage()
        {
            try
            {
                TagLib.File tags = null;
                tags = TagLib.File.Create(Path);
                if (tags.Tag.Pictures.Length > 0)
                {
                    var img = new BitmapImage();
                    img.BeginInit();
                    img.StreamSource = new MemoryStream(tags.Tag.Pictures[0].Data.Data);
                    img.EndInit();
                    return img;
                }
                else
                {
                    return new BitmapImage(DefaultImagePath);
                }
            }
            catch (FileNotFoundException)
            {
                return new BitmapImage(DefaultImagePath);
            }
        }

        public MusicTitle(String file)
        {
            var tags = TagLib.File.Create(file);
            Path = file;
            Artist = tags.Tag.FirstPerformer;
            Album = tags.Tag.Album;
            if (tags.Tag.FirstGenre != null)
                Genre = tags.Tag.FirstGenre;
            else
                Genre = "Inconnu";
            Year = tags.Tag.Year;
            TrackNumber = tags.Tag.Track;
            Title = tags.Tag.Title;
            Composer = tags.Tag.FirstComposer;
            Duration = tags.Properties.Duration;
            Type = ClickStyle.MUSIC;
            MessageColor = Colors.White;
        }

        public MusicTitle()
        {
        }

        public override Object Clone()
        {
            return new MusicTitle { Path = Path,
                                    Artist = Artist,
                                    Album = Album,
                                    Genre = Genre,
                                    Year = Year,
                                    TrackNumber = TrackNumber,
                                    Title = Title,
                                    Composer = Composer,
                                    Duration = Duration,
                                    Type = Type,
                                    MessageColor = Colors.White
            };

        }
    }
}
