using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsMedia.classes;
using Newtonsoft.Json;
using System.Windows;
using System.Windows.Media.Imaging;
using WindowsMedia.Properties;
using System.Drawing.Imaging;

namespace WindowsMedia
{
    class DefaultImageGetter
    {
        private static DefaultImageGetter instance = null;
        private static readonly object Lock = new object();
        static private Uri ImageDefaultPath = new Uri(@"assets/defaultimage.jpg", UriKind.Relative);
        static private Uri MovieDefaultPath = new Uri(@"assets/defaultvideoart.png", UriKind.Relative);

        public BitmapImage Image { get; private set; }
        public BitmapImage Movie { get; private set; }
        public BitmapImage Music { get; private set; }

        public static DefaultImageGetter Instance
        {
            get
            {
                lock (Lock)
                {
                    if (instance == null)
                    {
                        instance = new DefaultImageGetter();
                    }
                    return instance;
                }
            }
        }

        static public BitmapImage ConvertToBitmapImage(System.Drawing.Bitmap img)
        {
            BitmapImage result = null;
            using (MemoryStream memory = new MemoryStream())
            {
                img.Save(memory, ImageFormat.Bmp);
                memory.Position = 0;
                result = new BitmapImage();
                result.BeginInit();
                result.StreamSource = memory;
                result.CacheOption = BitmapCacheOption.OnLoad;
                result.EndInit();
                result.Freeze();
            }
            return result;
        }

        private DefaultImageGetter()
        {
            Image = ConvertToBitmapImage(Resources.DefaultPicImage);
            Movie = ConvertToBitmapImage(Resources.DefaultVideoImage);
            Music = ConvertToBitmapImage(Resources.DefaultMusicImage);
        }
    }
}
