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
            if (win.IsLoaded)
            { 
            win.GridMusicFilters.Visibility = System.Windows.Visibility.Visible;
            win.SecondBox.Visibility = System.Windows.Visibility.Visible;
            win.WrapBox.Visibility = System.Windows.Visibility.Hidden;
                win.MainBox.Visibility = System.Windows.Visibility.Visible;
            win.clickStyle_ = ClickStyle.MUSIC;
            win.WrapBox.ItemsSource = null;
            win.MainBox.ItemsSource = new AlbumIterator(win.lib_);
            win.MainBox.Visibility = System.Windows.Visibility.Visible;
        }
        }
        public void ImageSource(MainWindow win)
        {
            win.GridMusicFilters.Visibility = System.Windows.Visibility.Hidden;
            win.MainBox.Visibility = System.Windows.Visibility.Hidden;
            win.SecondBox.Visibility = System.Windows.Visibility.Hidden;
            win.WrapBox.Visibility = System.Windows.Visibility.Visible;
            win.SecondBox.ItemsSource = null;
            win.clickStyle_ = ClickStyle.IMAGE;
            win.WrapBox.ItemsSource = new ImageIterator(win.lib_);
            win.MainBox.ItemsSource = null;
        }
        public void VideoSource(MainWindow win)
        {
            win.GridMusicFilters.Visibility = System.Windows.Visibility.Hidden;
            win.MainBox.Visibility = System.Windows.Visibility.Hidden;
            win.SecondBox.Visibility = System.Windows.Visibility.Hidden;
            win.WrapBox.Visibility = System.Windows.Visibility.Visible;
            win.SecondBox.ItemsSource = null;
            win.clickStyle_ = ClickStyle.VIDEO;
            win.WrapBox.ItemsSource = new MovieIterator(win.lib_);
            win.MainBox.ItemsSource = null;
        }
    }
}
