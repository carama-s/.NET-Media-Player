using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace WindowsMedia.classes
{
    public class ImageFile
    {
        public String Path { get; private set; }
        public String Title { get; private set; }
        public TimeSpan Duration { get; private set; }
        public String Description { get; private set; }
        public BitmapImage Image { get; private set; }

        public ImageFile(String path)
        {
            Path = path;
            this.Duration = new TimeSpan(3);
            this.Title = Path.Substring(0, Path.LastIndexOf('.')).Split("\\".ToCharArray()).Last();
            this.Image = new BitmapImage(new Uri(Path, UriKind.Absolute));
        }
    }
}
