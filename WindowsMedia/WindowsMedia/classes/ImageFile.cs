using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WindowsMedia.classes
{
    public class ImageFile : MediaItem, INotifyPropertyChanged
    {
        static public Uri DefaultImagePath = new Uri("../assets/defaultimage.jpg", UriKind.Relative);
        public String Description { get; private set; }
        private BitmapImage GeneratedImage { get; set; }

        protected override BitmapImage GetImage()
        {
            if (GeneratedImage == null)
            {
                ThreadPool.QueueUserWorkItem(BackgroundGenerateImage, null);
                return new BitmapImage(DefaultImagePath);
            }
            else
                return GeneratedImage;
        }

        private void BackgroundGenerateImage(object param)
        {
            GeneratedImage = new BitmapImage(new Uri(Path, UriKind.Absolute));
            GeneratedImage.Freeze();
            OnPropertyChanged("Image");
        }

        public ImageFile(String path)
        {
            GeneratedImage = null;
            Path = path;
            Artist = "";
            Duration = TimeSpan.FromSeconds(0);
            Title = System.IO.Path.GetFileNameWithoutExtension(path);
            Type = ClickStyle.IMAGE;
            MessageColor = Colors.White;
        }

        public ImageFile()
        {
        }

        public override Object Clone()
        {
            return new ImageFile { Path = Path,
                                   Artist = Artist, 
                                   Duration = Duration, 
                                   Title = Title, 
                                   Type = Type, 
                                   MessageColor = Colors.White,
                                   GeneratedImage = GeneratedImage };
        }
    }
}
