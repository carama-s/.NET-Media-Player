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
        public BitmapImage Image { get; private set; }
        public MovieFile(String path)
        {
            Path = path;
            var tags = TagLib.File.Create(path);
            Duration = tags.Properties.Duration;
            var name = (String)Path.Split("\\".ToCharArray()).Last();
            Title = name.Substring(0, name.LastIndexOf('.'));
            this.Image = new BitmapImage(new Uri("../assets/defaultvideoart.png", UriKind.Relative));
        }
    }
}
