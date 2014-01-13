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
        public String Album { get; private set; }
        public String Genre { get; private set; }
        public uint Year { get; private set; }
        public uint TrackNumber { get; private set; }
        public String Composer { get; private set; }

        public MusicTitle(String file)
        {
            var tags = TagLib.File.Create(file);
            Path = file;
            Artist = (tags.Tag.FirstPerformer != null) ? tags.Tag.FirstPerformer : "Inconnu";
            Album = (tags.Tag.Album != null) ? tags.Tag.Album : "Inconnu";
            Genre = (tags.Tag.FirstGenre != null) ? tags.Tag.FirstGenre : "Inconnu";
            Year = (tags.Tag.Year != null) ? tags.Tag.Year : 0;
            TrackNumber = (tags.Tag.Track != null) ? tags.Tag.Track : 0;
            Title = (tags.Tag.Title != null) ? tags.Tag.Title : System.IO.Path.GetFileNameWithoutExtension(Path);
            Composer = (tags.Tag.FirstComposer != null) ? tags.Tag.FirstComposer : "Inconnu";
            Duration = tags.Properties.Duration;
            Type = ClickStyle.MUSIC;
            MessageColor = Colors.White;
            try
            {
                if (tags.Tag.Pictures.Length > 0)
                {
                    Image = new BitmapImage();
                    Image.BeginInit();
                    Image.StreamSource = new MemoryStream(tags.Tag.Pictures[0].Data.Data);
                    Image.EndInit();
                }
                else
                {
                    Image = DefaultImageGetter.Instance.Music;
                }
            }
            catch (Exception)
            {
                Image = DefaultImageGetter.Instance.Music;
            }
            Image.Freeze();
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
                                    MessageColor = Colors.White,
                                    Image = Image
            };

        }
    }
}
