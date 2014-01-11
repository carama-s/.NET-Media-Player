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
        public String Description { get; private set; }

        public ImageFile(String path)
        {
            Path = path;
            Artist = "";
            Duration = TimeSpan.FromSeconds(0);
            Title = System.IO.Path.GetFileNameWithoutExtension(path);
            Type = ClickStyle.IMAGE;
            MessageColor = Colors.White;
            try
            {
                Image = new BitmapImage(new Uri(Path, UriKind.Absolute));
            }
            catch (NotSupportedException)
            {
                Image = DefaultImageGetter.Instance.Image;
            }
            Image.Freeze();
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
                                   Image = Image };
        }
    }
}
