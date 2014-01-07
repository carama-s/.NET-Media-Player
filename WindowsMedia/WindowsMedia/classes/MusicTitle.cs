using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
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

        protected override BitmapImage GetImage()
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
            BrushText = Brushes.White;
        }
    }
}
