using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WindowsMedia.classes
{
    public class DelegateSourceClass
    {
        delegate void DelegateTemplate(MainWindow win);

        public void PlaylistSource(MainWindow win)
        {
            win.GridMusicFilters.Visibility = System.Windows.Visibility.Hidden;
            win.SecondBox.Visibility = System.Windows.Visibility.Visible;
            win.WrapBox.Visibility = System.Windows.Visibility.Hidden;
            win.MainBox.Visibility = System.Windows.Visibility.Visible;
            win.clickStyle_ = ClickStyle.SELECTION;
            win.WrapBox.ItemsSource = null;
            win.SecondBox.ItemsSource = null;
            win.MainBox.ItemsSource = win.lib_.Playlists;
            win.MainBox.ItemTemplate = win.FindResource("MainSelectionTemplate") as DataTemplate;
            win.MainBox.Visibility = System.Windows.Visibility.Visible;
        }
        public void MusicSource(MainWindow win)
        {
            if (win.IsLoaded)
            {
                win.GridMusicFilters.Visibility = System.Windows.Visibility.Visible;
                win.SecondBox.Visibility = System.Windows.Visibility.Visible;
                win.WrapBox.Visibility = System.Windows.Visibility.Hidden;
                win.MainBox.Visibility = System.Windows.Visibility.Visible;
                win.SecondBox.ItemsSource = null;
                win.clickStyle_ = ClickStyle.MUSIC;
                win.WrapBox.ItemsSource = null;
                win.MainBox.Visibility = System.Windows.Visibility.Visible;
                DelegateTemplate[] tab = new DelegateTemplate[3];
                tab[0] = new DelegateTemplate(this.AlbumTemplate);
                tab[1] = new DelegateTemplate(this.ArtistTemplate);
                tab[2] = new DelegateTemplate(this.GenreTemplate);
                tab[(int)win.musicStyle_](win);
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
        void AlbumTemplate(MainWindow win)
        {
            win.MainBox.ItemsSource = new AlbumIterator(win.lib_);
            win.MainBox.ItemTemplate = win.FindResource("MainAlbumTemplate") as DataTemplate;
        }
        void ArtistTemplate(MainWindow win)
        {
            win.MainBox.ItemsSource = new ArtistIterator(win.lib_);
            win.MainBox.ItemTemplate = win.FindResource("MainArtistTemplate") as DataTemplate;
        }
        void GenreTemplate(MainWindow win)
        {
            win.MainBox.ItemsSource = new GenreIterator(win.lib_);
            win.MainBox.ItemTemplate = win.FindResource("MainGenreTemplate") as DataTemplate;
        }
    }
}
