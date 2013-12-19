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
            win.GridMusicFilters.Visibility = System.Windows.Visibility.Hidden;
            win.SecondBox.ItemsSource = null;
        }
        public void MusicSource(MainWindow win)
        {
            win.GridMusicFilters.Visibility = System.Windows.Visibility.Visible;
            win.PanoramaDisplay.Visibility = System.Windows.Visibility.Hidden;
            win.MainBox.Visibility = System.Windows.Visibility.Visible;
            win.SecondBox.Visibility = System.Windows.Visibility.Visible;
            win.clickStyle_ = ClickStyle.MUSIC;
            win.MainBox.ItemsSource = win.musicLib_;
        }
        public void ImageSource(MainWindow win)
        {
            win.GridMusicFilters.Visibility = System.Windows.Visibility.Hidden;
            win.SecondBox.ItemsSource = null;
        }
        public void VideoSource(MainWindow win)
        {
            win.GridMusicFilters.Visibility = System.Windows.Visibility.Hidden;
            win.MainBox.Visibility = System.Windows.Visibility.Hidden;
            win.PanoramaDisplay.Visibility = System.Windows.Visibility.Visible;
            win.SecondBox.Visibility = System.Windows.Visibility.Hidden;
            win.SecondBox.ItemsSource = null;
            win.clickStyle_ = ClickStyle.VIDEO;
            win.PanoramaDisplay.ItemsSource = win.movieLib_;
            win.MainBox.ItemsSource = null;
        }
    }
}
