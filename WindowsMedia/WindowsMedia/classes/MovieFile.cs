using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WindowsMedia.classes
{
    public class MovieFile : MediaItem
    {
        public String Description { get; private set; }

        protected override BitmapImage GetImage()
        {
            return new BitmapImage(new Uri("../assets/defaultvideoart.png", UriKind.Relative));
        }

        public MovieFile(String path)
        {
            var tags = TagLib.File.Create(path);
            Path = path;
            Artist = "";
            Duration = tags.Properties.Duration;
            Title = tags.Name.Substring(0,tags.Name.LastIndexOf('.')).Split("\\".ToCharArray()).Last();
            Type = ClickStyle.VIDEO;
            MessageColor = Colors.White;
        }

        public MovieFile()
        {
        }

        public override Object Clone()
        {
            return new MovieFile { Path = Path,
                                   Artist = Artist,
                                   Duration = Duration,
                                   Title = Title,
                                   Type = Type,
                                   MessageColor = Colors.White };
        }
    }
}
