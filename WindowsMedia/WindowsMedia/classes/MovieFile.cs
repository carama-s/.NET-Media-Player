using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;

namespace WindowsMedia.classes
{
    public class MovieFile
    {
        public String Path { get; private set; }
        public String Title { get; private set; }
        public TimeSpan Duration { get; private set; }
        public String Description { get; private set; }
        public BitmapImage Image { get; private set; }

        public MovieFile(String path)
        {
            Path = path;
            var tags = TagLib.File.Create(path);
            this.Duration = tags.Properties.Duration;
            this.Title = tags.Name.Substring(0,tags.Name.LastIndexOf('.')).Split("\\".ToCharArray()).Last();
            this.Image = new BitmapImage(new Uri("../assets/defaultvideoart.png", UriKind.Relative));
        }
    }
}
