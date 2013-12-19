using WindowsMedia.classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Resources;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Collections.ObjectModel;
using System.IO;
using MahApps.Metro.Controls;
using MahApps.Metro;
using Microsoft.Win32;

namespace WindowsMedia
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    
    public enum State { PLAY, STOP, PAUSE };
    public enum MusicStyle { ALBUM, ARTIST, GENRE };
    public enum ClickStyle { SELECTION, MUSIC, IMAGE, VIDEO };

    public partial class MainWindow : MetroWindow
    {
        private TimeSpan        duree_;
        private String          source_;
        private State           state_;
        public MusicStyle       musicStyle_;
        public ClickStyle       clickStyle_;
        private bool            isMuted_;
        private bool            isFullScreen_;
        private bool            isRepeat_;
        private bool            isShuffle_;
        private DispatcherTimer timer_;
        private double          oldValue;
        delegate void           DelegateSource(MainWindow win);
        private double          oldSize_;
        public MusicLibrary     musicLib_;
        public MovieLibrary     movieLib_;
 
        public MainWindow()
        {
            this.oldSize_ = -1;
            this.Loaded += MainWindow_Loaded;
            InitializeComponent();
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            BrushConverter bc = new BrushConverter();

            this.ButtonAlbums.Foreground = (Brush)bc.ConvertFrom("#FF41B1E1");

            this.isMuted_ = false;
            this.isFullScreen_ = false;
            this.isRepeat_ = false;
            this.isShuffle_ = false;
            this.oldValue = -1;
            this.state_ = State.STOP;
            this.clickStyle_ = ClickStyle.MUSIC;
            this.musicStyle_ = MusicStyle.ALBUM;
            this.MediaPlayer.LoadedBehavior = MediaState.Manual;
            this.MediaPlayer.UnloadedBehavior = MediaState.Manual;
            List<MenuTemplateClass> box = new List<MenuTemplateClass>();
            box.Add(new MenuTemplateClass(" SELECTIONS", ""));
            box.Add(new MenuTemplateClass(" MUSIQUES", ""));
            box.Add(new MenuTemplateClass(" IMAGES", ""));
            box.Add(new MenuTemplateClass(" VIDEOS", ""));
            BoxSelectMedia.ItemsSource = box;

            this.SliderVolume.Value = 50;
            this.SliderTime.Maximum = this.Width - 160;
            this.SliderTime.IsMoveToPointEnabled = true;

            musicLib_ = new MusicLibrary(new List<string> { Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) });
            musicLib_.GenerateLibrary();
            MainBox.ItemsSource = musicLib_;
            movieLib_ = new MovieLibrary(new List<string> { Environment.GetFolderPath(Environment.SpecialFolder.MyVideos)});
            movieLib_.GenerateLibrary();
        }

        // Gestion bouton Play/Pause
        private void ButtonPlay_Click(object sender, RoutedEventArgs e)
        {
            ImageBrush brush;
            if ((this.state_ == State.STOP || this.state_ == State.PAUSE) && (MediaPlayer.Source != null))
            {
                brush = createBrush("assets/icon-pause-barre.png");
                this.state_ = State.PLAY;
                this.MediaPlayer.Play();
                if (oldValue == -1)
                {
                    this.CurrentTime.Text = "00:00:00";
                    this.SliderTime.Value = 0;
                }
                var tags = TagLib.File.Create(this.source_);
                this.duree_ = tags.Properties.Duration;
                this.MediaPlayer.Visibility = System.Windows.Visibility.Visible;
                this.TotalTime.Text = this.duree_.ToString();
                this.timer_ = new DispatcherTimer();
                this.timer_.Interval = TimeSpan.FromMilliseconds(100);
                this.timer_.Tick += new EventHandler(timer_Tick);
                this.timer_.Start();
            }
            else // this.state_ == State.PLAY
            {
                brush = createBrush("assets/icon-play-barre.png");
                this.state_ = State.PAUSE;
                this.MediaPlayer.Pause();
            }
            this.ButtonPlay.Background = brush;
            this.ButtonPlay.OpacityMask = brush;
        }

        // Gestion bouton Stop
        private void ButtonStop_Click(object sender, RoutedEventArgs e)
        {
            if (this.state_ == State.PAUSE || this.state_ == State.PLAY)
            {
                ImageBrush brush = createBrush("assets/icon-play-barre.png");
                this.ButtonPlay.Background = brush;
                this.ButtonPlay.OpacityMask = brush;
                this.state_ = State.STOP;
                this.SliderTime.Value = 0;
                this.oldValue = -1;
                this.MediaPlayer.Stop();
                this.MediaPlayer.Visibility = Visibility.Hidden;
            }
        }

        // Gestion du bouton Volume
        private void ButtonVolume_Click(object sender, RoutedEventArgs e)
        {
            ImageBrush brush;

            if (this.isMuted_)
            {
                brush = createBrushVolume(this.SliderVolume.Value);
                this.MediaPlayer.Volume = (double)(this.SliderVolume.Value / 100);
                this.isMuted_ = false;
            }
            else
            {
                brush = createBrushVolume(0);
                this.MediaPlayer.Volume = 0;
                this.isMuted_ = true;
            }
            this.ButtonVolume.Background = brush;
            this.ButtonVolume.OpacityMask = brush;
        }

        // Gestion du bouton Switch Frame
        private void ButtonSwitch_Click(object sender, RoutedEventArgs e)
        {
            if (GridLecture.Visibility == System.Windows.Visibility.Hidden)
            {
                GridLecture.Visibility = System.Windows.Visibility.Visible;
                GridBibliotheque.Visibility = System.Windows.Visibility.Hidden;
            }
            else
            {
                if (this.isFullScreen_ == true)
                    this.FulllScreenOff();
                GridBibliotheque.Visibility = System.Windows.Visibility.Visible;
                GridLecture.Visibility = System.Windows.Visibility.Hidden;
            }
        }

        // Gestion du bouton Back
        private void BackButtonMediaElement(object sender, RoutedEventArgs e)
        {
                var List = this.SecondBox.Items;
                List.MoveCurrentToPrevious();
                if (List.IsCurrentBeforeFirst)
                    List.MoveCurrentToLast();
                MusicTitle music = (MusicTitle)List.CurrentItem;
                this.source_ = music.Path;
                this.MediaPlayer.Source = new Uri(this.source_, UriKind.RelativeOrAbsolute);
                this.state_ = State.STOP;
                this.ButtonPlay_Click(sender, e);
            }

        // Gestion du bouton Forward
        private void ForwardButtonMediaElement(object sender, RoutedEventArgs e)
        {
                var List = this.SecondBox.Items;
                List.MoveCurrentToNext();
                if (List.IsCurrentAfterLast)
                    List.MoveCurrentToFirst();
                MusicTitle music = (MusicTitle)List.CurrentItem;
                this.source_ = music.Path;
                this.MediaPlayer.Source = new Uri(this.source_, UriKind.RelativeOrAbsolute);
                this.state_ = State.STOP;
                this.ButtonPlay_Click(sender, e);
            }

        // Gestion du Slide de la video
        void timer_Tick(object sender, EventArgs e)
        {
            double value = (double)((this.MediaPlayer.Position.Hours * 3600) + (this.MediaPlayer.Position.Minutes * 60) + this.MediaPlayer.Position.Seconds) / (double)this.duree_.TotalSeconds;
            oldValue = value * (double)this.SliderTime.Maximum;
            this.SliderTime.Value = oldValue;
            this.CurrentTime.Text = this.MediaPlayer.Position.ToString();
        }

        // Gestion du Slide du volume
        private void SliderVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ImageBrush brush;

            brush = createBrushVolume(this.SliderVolume.Value);

            this.ButtonVolume.Background = brush;
            this.ButtonVolume.OpacityMask = brush;
            this.MediaPlayer.Volume = (double)(this.SliderVolume.Value / 100);
        }

        // Gestion de la valeur du curseur du Slide
        private void SliderTime_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (this.oldSize_ > (this.Width - 160))
                this.timer_Tick(sender, e);
            else
            {
                double OldPosition = (oldValue * (double)this.duree_.TotalSeconds) / this.SliderTime.Maximum;
                double SliderValue = (double)SliderTime.Value;
                double Position = (SliderValue * (double)this.duree_.TotalSeconds) / this.SliderTime.Maximum;
                if (OldPosition != Position)
                    this.MediaPlayer.Position = TimeSpan.FromSeconds(Position);
            }
        }

        //fullscren fonction
        private void FullScreenOn()
        {
            this.ShowTitleBar = false;
            this.IgnoreTaskbarOnMaximize = true;
            this.WindowStyle = WindowStyle.None;
            this.WindowState = WindowState.Maximized;
            this.MediaPlayer.Stretch = Stretch.Uniform;
            this.SliderTime.Maximum = this.Width - 160;
            this.isFullScreen_ = true;
        }

        private void FulllScreenOff()
        {
            this.ShowTitleBar = true;
            this.IgnoreTaskbarOnMaximize = false;
            this.WindowState = WindowState.Normal;
            this.MediaPlayer.Stretch = Stretch.Uniform;
            this.SliderTime.Maximum = this.Width - 160;
            this.isFullScreen_ = false;
        }

        // Gestion du FullScreen
        private void EventClicMediaElement(object sender, MouseButtonEventArgs e)
        {

            if (this.isFullScreen_ == false && e.ClickCount == 2)
                this.FullScreenOn();
            else if (this.isFullScreen_ == true && e.ClickCount == 2)
                this.FulllScreenOff();
        }

        // Gestion de la modification de la MainWindow
        private void MainWindowUpdated(object sender, EventArgs e)
        {
            this.oldSize_ = (double) this.SliderTime.Maximum;
            this.SliderTime.Maximum = this.Width - 160;
        }

        // Gestion Brush
        public static ImageBrush createBrush(string path)
        {
            Uri resourceUri = new Uri(path, UriKind.Relative);
            StreamResourceInfo streamInfo = Application.GetResourceStream(resourceUri);

            BitmapFrame temp = BitmapFrame.Create(streamInfo.Stream);
            ImageBrush brush = new ImageBrush();
            brush.ImageSource = temp;

            return (brush);
        }

        public static ImageBrush createBrushVolume(double value)
        {
            ImageBrush brush;

            if (value == 0) // A voir si on met isMuted_ à TRUE?
                brush = createBrush("assets/icon-volumemute-barre.png");
            else if (value >= 0.1 && value < 34)
                brush = createBrush("assets/icon-volume1-barre.png");
            else if (value >= 34 && value <= 67)
                brush = createBrush("assets/icon-volume2-barre.png");
            else
                brush = createBrush("assets/icon-volumemax-barre.png");
            return (brush);
        }

        // Gestion fin de Media Element
        private void EventEndMedia(object sender, RoutedEventArgs e)
        {
            this.ButtonStop_Click(sender, e);
        }

        // Gestion Box bibliothèque
        private void MainBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                switch(this.clickStyle_)
                {
                    case (ClickStyle.SELECTION):
                        {
                            break;
                        }
                    case (ClickStyle.MUSIC):
                        {
                            MusicAlbum al = (MusicAlbum)e.AddedItems[0];
                            SecondBox.ItemsSource = al;
                            break;
                        }
                    case (ClickStyle.IMAGE):
                        {
                            break;
                        }
                    case (ClickStyle.VIDEO):
                        {
                            MovieFile mv = (MovieFile)e.AddedItems[0];
                            this.source_ = mv.Path;
                            this.MediaPlayer.Source = new Uri(mv.Path, UriKind.RelativeOrAbsolute);
                            break;
                        }
                    default:
                        break;
                }
            }
        }

        private void MainBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (MainBox.SelectedItems.Count > 0 && clickStyle_ != ClickStyle.MUSIC)
            {
                switch (this.clickStyle_)
                {
                    case (ClickStyle.SELECTION):
                        {
                            break;
                        }
                    case (ClickStyle.IMAGE):
                        {
                            break;
                        }
                    case (ClickStyle.VIDEO):
                        {
                            MovieFile mv = (MovieFile)MainBox.SelectedItem;
                            this.source_ = mv.Path;
                            this.MediaPlayer.Source = new Uri(mv.Path, UriKind.RelativeOrAbsolute);
                            this.state_ = State.STOP;
                            ButtonPlay_Click(sender, e);
                            ButtonSwitch_Click(sender, e);
                            break;
                        }
                    default:
                        break;
                }
            }
        }

        private void SecondBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                MusicTitle ti = (MusicTitle)e.AddedItems[0];
                this.MediaPlayer.Source = new Uri(ti.Path, UriKind.RelativeOrAbsolute);
                this.source_ = ti.Path;
            }
        }

        private void SecondBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (SecondBox.SelectedItems.Count > 0)
            {
                MusicTitle ti = (MusicTitle)SecondBox.SelectedItem;
                this.source_ = ti.Path;
                this.MediaPlayer.Source = new Uri(ti.Path, UriKind.RelativeOrAbsolute);
                this.state_ = State.STOP;
                ButtonPlay_Click(sender, e);
            }
        }

        private void BoxSelectMedia_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DelegateSourceClass del = new DelegateSourceClass();
            DelegateSource[] bind = new DelegateSource[4];
            bind[0] = new DelegateSource(del.PlaylistSource);
            bind[1] = new DelegateSource(del.MusicSource);
            bind[2] = new DelegateSource(del.ImageSource);
            bind[3] = new DelegateSource(del.VideoSource);
            if (BoxSelectMedia.SelectedIndex != -1)
                bind[BoxSelectMedia.SelectedIndex](this);
        }

        private void ButtonRepeat_Click(object sender, RoutedEventArgs e)
        {
            ImageBrush brush;

            if (this.isRepeat_)
            {
                this.isRepeat_ = false;
                brush = createBrush("assets/icon-repeat-barre.png");
            }
            else
            {
                this.isRepeat_ = true;
                brush = createBrush("assets/icon-enable-repeat-barre.png");
            }
            this.ButtonRepeat.Background = brush;
            this.ButtonRepeat.OpacityMask = brush;
        }

        private void ButtonShuffle_Click(object sender, RoutedEventArgs e)
        {
            ImageBrush brush;

            if (this.isShuffle_)
            {
                this.isShuffle_ = false;
                brush = createBrush("assets/icon-shuffle-barre.png");
            }
            else
            {
                this.isShuffle_ = true;
                brush = createBrush("assets/icon-enable-shuffle-barre.png");
            }
            this.ButtonShuffle.Background = brush;
            this.ButtonShuffle.OpacityMask = brush;
        }

        private void SliderTime_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (this.oldSize_ > (this.Width - 160))
                this.timer_Tick(sender, e);
            else
            {
                double OldPosition = (oldValue * (double)this.duree_.TotalSeconds) / this.SliderTime.Maximum;
                double SliderValue = (double)SliderTime.Value;
                double Position = (SliderValue * (double)this.duree_.TotalSeconds) / this.SliderTime.Maximum;
                if (OldPosition != Position)
                    this.MediaPlayer.Position = TimeSpan.FromSeconds(Position);
            }
        }
        
        private void ResetSelectionFilterMusic()
        {
            BrushConverter bc = new BrushConverter();

            this.ButtonAlbums.Foreground = (Brush)bc.ConvertFrom("#FFFFFFFF");
            this.ButtonArtists.Foreground = (Brush)bc.ConvertFrom("#FFFFFFFF");
            this.ButtonGenres.Foreground = (Brush)bc.ConvertFrom("#FFFFFFFF");
        }

        private void ButtonAlbums_Click(object sender, RoutedEventArgs e)
        {
            BrushConverter bc = new BrushConverter();

            ResetSelectionFilterMusic();
            this.ButtonAlbums.Foreground = (Brush)bc.ConvertFrom("#FF41B1E1");
            this.musicStyle_ = MusicStyle.ALBUM;
        }

        private void ButtonArtists_Click(object sender, RoutedEventArgs e)
        {
            BrushConverter bc = new BrushConverter();

            ResetSelectionFilterMusic();
            this.ButtonArtists.Foreground = (Brush)bc.ConvertFrom("#FF41B1E1");
            this.musicStyle_ = MusicStyle.ARTIST;
        }

        private void ButtonGenres_Click(object sender, RoutedEventArgs e)
        {
            BrushConverter bc = new BrushConverter();

            ResetSelectionFilterMusic();
            this.ButtonGenres.Foreground = (Brush)bc.ConvertFrom("#FF41B1E1");
            this.musicStyle_ = MusicStyle.GENRE;
        }

        private void PlayItem(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "MP3/MP4/MKV/AVI/JPEG/PNG/JPG Files (*.mp3, *.mp4, *.avi, *.jpeg, *.png, *.jpg) | *.mp3; *.mp4; *.avi; *.jpeg; *.png; *.jpg"; 
            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                this.source_ = dlg.FileName;
                MediaPlayer.Stop();
                MediaPlayer.Source = new Uri(this.source_, UriKind.RelativeOrAbsolute);
                this.state_ = State.STOP;
                this.ButtonPlay_Click(sender, e);
            }
        }

        private void OpenFile(object sender, RoutedEventArgs e)
        {
  
            System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog();            
            dlg.ShowDialog();
            string filename = dlg.SelectedPath;
            Console.Out.WriteLine(filename);
        }

        private void PanoramaDisplay_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }

        private void PanoramaDisplay_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }
    }
}
