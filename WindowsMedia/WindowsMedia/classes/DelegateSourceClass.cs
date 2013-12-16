using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsMedia.classes
{
    public class DelegateSourceClass
    {
        public void PlaylistSource(MainWindow win)
        {
        }
        public void MusicSource(MainWindow win)
        {
            win.MainBox.ItemsSource = win.musicLib_;
        }
        public void ImageSource(MainWindow win)
        {
        }
        public void VideoSource(MainWindow win)
        {
            win.MainBox.ItemsSource = win.movieLib_;
        }
    }
}
