﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace WindowsMedia.classes
{
    public class ImageFile : MediaItem
    {
        public String Description { get; private set; }

        protected override BitmapImage GetImage()
        {
            return new BitmapImage(new Uri(Path, UriKind.Absolute));
        }

        public ImageFile(String path)
        {
            Path = path;
            Artist = "";
            Duration = TimeSpan.FromSeconds(0);
            Title = Path.Substring(0, Path.LastIndexOf('.')).Split("\\".ToCharArray()).Last();
        }
    }
}
