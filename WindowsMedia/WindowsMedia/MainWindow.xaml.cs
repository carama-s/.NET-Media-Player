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
using System.Diagnostics;
using MyToolkit.Multimedia;
using System.Net.Http;

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
        private String          sourceVideo_;
        private TimeSpan        duree_;
        private String          source_;
        private State           state_;
        private String          typeCurrentMedia_;
        public MusicStyle       musicStyle_;
        public ClickStyle       clickStyle_;
        private bool            isMuted_;
        private bool            isFullScreen_;
        private bool            isRepeat_;
        private bool            isShuffle_;
        private DispatcherTimer timer_text;
        private DispatcherTimer timer_Slide;
        private DispatcherTimer timer_3;
        private double          oldValue;
        delegate void           DelegateSource(MainWindow win);
        private double          oldSize_;
        public Library          lib_;

        private int currentIndexLecture_;

 
        public MainWindow()
        {
            this.oldSize_ = -1;
            this.Loaded += MainWindow_Loaded;
            AddHandler(Keyboard.KeyDownEvent, (KeyEventHandler)WindowKeyDown);

            InitializeComponent();
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            BrushConverter bc = new BrushConverter();

            this.ButtonAlbums.Foreground = (Brush)bc.ConvertFrom("#FF41B1E1");

            this.timer_Slide = new DispatcherTimer();
            this.timer_Slide.Interval = TimeSpan.FromMilliseconds(100);
            this.timer_Slide.Tick += new EventHandler(timer_Tick);
            
            this.timer_3 = new DispatcherTimer();
            this.timer_3.Interval = TimeSpan.FromSeconds(2);
            this.timer_3.Tick += new EventHandler(timer_tick_Slide);
            
            this.timer_text = new DispatcherTimer();
            this.timer_text.Interval = TimeSpan.FromMilliseconds(100);
            this.timer_text.Tick += new EventHandler(timer_Text);
            
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

            this.SliderVolume.Value = 50;
            this.SliderTime.Maximum = this.Width - 160;
            this.SliderTime.IsMoveToPointEnabled = true;

            var watch = Stopwatch.StartNew();

            lib_ = new Library();
            lib_.GenerateLibrary();
            MainBox.ItemsSource = new AlbumIterator(lib_);
        }

        private void WindowKeyDown(object sender, KeyEventArgs e)
        {
            if (isFullScreen_ == true && e.Key == Key.Escape)
                FulllScreenOff();
            else if ((e.Key == Key.O) && Keyboard.IsKeyDown(Key.LeftCtrl))
                PlayItem(sender, e);
            else if ((e.Key == Key.I) && Keyboard.IsKeyDown(Key.LeftCtrl))
                OpenFile(sender, e);
        }

        private void timer_Text(object sender, EventArgs e)
        {
            String letter = this.NomVideo.Text.Substring(0, 1);
            this.NomVideo.Text = this.NomVideo.Text.Remove(0, 1);
            this.NomVideo.Text += letter;
        }

        // Gestion bouton Play/Pause
        private void ButtonPlay_Click(object sender, RoutedEventArgs e)
        {
            ImageBrush brush;

            if ((this.state_ == State.STOP || this.state_ == State.PAUSE))
            {
                if (PlaylistBox.Items.Count > 0 && this.state_ != State.PAUSE)
                {
                    MediaItem item = (MediaItem)PlaylistBox.SelectedItem;
                    this.source_ = item.Path;
                    this.MediaPlayer.Source = new Uri(item.Path, UriKind.RelativeOrAbsolute);
                    this.PlayingItemImage.Source = item.Image;
                    this.PlayingItemTitle.Text = item.Title;
                    this.PlayingItemArtist.Text = item.Artist;
                }
                if (MediaPlayer.Source != null)
                {
                    brush = createBrush("assets/icon-pause-barre.png");
                    this.currentIndexLecture_ = PlaylistBox.SelectedIndex;
                    this.state_ = State.PLAY;
                    this.MediaPlayer.Play();
                    this.NomVideo.Width = 150;
                    if (oldValue == -1)
                    {
                        this.CurrentTime.Text = "00:00:00";
                        this.SliderTime.Value = 0;
                    }
                    String[] media = this.source_.Split('\\');
                    this.NomVideo.Text = media[media.Length - 1] + "     ";
                    this.timer_text.Start();
                    var tags = TagLib.File.Create(this.source_);
                    this.typeCurrentMedia_ = tags.Properties.MediaTypes.ToString();
                    this.duree_ = tags.Properties.Duration;
                    this.MediaPlayer.Visibility = System.Windows.Visibility.Visible;

                    this.TotalTime.Text = this.duree_.ToString();
                    this.timer_Slide.Start();

            }
                else
                    brush = createBrush("assets/icon-play-barre.png");
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
                this.NomVideo.Text = "";
                this.state_ = State.STOP;
                this.SliderTime.Value = 0;
                this.oldValue = -1;
                this.timer_text.Stop();
                this.NomVideo.Text = "";
                this.MediaPlayer.Stop();
                Mouse.OverrideCursor = null;
                this.MediaPlayer.Visibility = Visibility.Visible;
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
            if (PlaylistBox.Items.Count > 0)
            {
                if (PlaylistBox.SelectedIndex == 0)
                    PlaylistBox.SelectedIndex = PlaylistBox.Items.Count - 1;
                else
                    PlaylistBox.SelectedIndex -= 1;

                this.currentIndexLecture_ = PlaylistBox.SelectedIndex;
                this.source_ = null;
                this.state_ = State.STOP;
                this.ButtonPlay_Click(sender, e);
            }
        }

        // Gestion du bouton Forward
        private void ForwardButtonMediaElement(object sender, RoutedEventArgs e)
        {
            if (PlaylistBox.Items.Count > 0)
            {
                if (PlaylistBox.SelectedIndex == (PlaylistBox.Items.Count - 1))
                    PlaylistBox.SelectedIndex = 0;
                else
                    PlaylistBox.SelectedIndex += 1;

                this.currentIndexLecture_ = PlaylistBox.SelectedIndex;
                this.source_ = null;
                this.state_ = State.STOP;
                this.ButtonPlay_Click(sender, e);
            }
        }

        // Gestion du Slide de la video
        void timer_Tick(object sender, EventArgs e)
        {
            if (typeCurrentMedia_ != "Photo")
            {
                double value = (double)((this.MediaPlayer.Position.Hours * 3600) + (this.MediaPlayer.Position.Minutes * 60) + this.MediaPlayer.Position.Seconds) / (double)this.duree_.TotalSeconds;
                oldValue = value * (double)this.SliderTime.Maximum;
                this.SliderTime.Value = oldValue;
                this.CurrentTime.Text = this.MediaPlayer.Position.ToString();
            }
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

            if (PlaylistBox.SelectedIndex < (PlaylistBox.Items.Count - 1))
                ForwardButtonMediaElement(sender, e);
            else if (isRepeat_)
                ForwardButtonMediaElement(sender, e);
            else
            {
                PlaylistBox.SelectedIndex = 0;
                this.currentIndexLecture_ = PlaylistBox.SelectedIndex;
                this.source_ = null;
                this.state_ = State.STOP;
        }

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
                            var al = (List<MusicTitle>)e.AddedItems[0];
                            SecondBox.ItemsSource = al;
                            break;
                        }
                    default:
                        break;
                }
            }
        }

        private void MainBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (MainBox.SelectedItems.Count > 0)
            {
                PlaylistBox.Items.Clear();
                var items = (List<MusicTitle>)MainBox.SelectedItems[0];
                foreach (var title in items)
                    PlaylistBox.Items.Add(title);
                PlaylistBox.SelectedIndex = 0;
                this.state_ = State.STOP;

                ButtonPlay_Click(sender, e);
            }
        }

        private void SecondBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            }

        private void SecondBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            PlaylistBox.Items.Clear();
            if (SecondBox.SelectedItems.Count > 0)
            {
                var items = (List<MusicTitle>)MainBox.SelectedItems[0];
                foreach (var title in items)
                    PlaylistBox.Items.Add(title);
                PlaylistBox.SelectedIndex = SecondBox.SelectedIndex;
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
            this.MainBox.ItemsSource = new AlbumIterator(lib_);
            this.MainBox.SelectedIndex = -1;
            this.SecondBox.ItemsSource = null;
            this.MainBox.ItemTemplate = FindResource("MainAlbumTemplate") as DataTemplate;
        }

        private void ButtonArtists_Click(object sender, RoutedEventArgs e)
        {
            BrushConverter bc = new BrushConverter();

            ResetSelectionFilterMusic();
            this.ButtonArtists.Foreground = (Brush)bc.ConvertFrom("#FF41B1E1");
            this.musicStyle_ = MusicStyle.ARTIST;
            this.MainBox.ItemsSource = new ArtistIterator(lib_);
            this.MainBox.SelectedIndex = -1;
            this.SecondBox.ItemsSource = null;
            this.MainBox.ItemTemplate = FindResource("MainArtistTemplate") as DataTemplate;
        }

        private void ButtonGenres_Click(object sender, RoutedEventArgs e)
        {
            BrushConverter bc = new BrushConverter();

            ResetSelectionFilterMusic();
            this.ButtonGenres.Foreground = (Brush)bc.ConvertFrom("#FF41B1E1");
            this.musicStyle_ = MusicStyle.GENRE;
            this.MainBox.ItemsSource = new GenreIterator(lib_);
            this.MainBox.SelectedIndex = -1;
            this.SecondBox.ItemsSource = null;
            this.MainBox.ItemTemplate = FindResource("MainGenreTemplate") as DataTemplate;
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

        private void WrapBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                switch (this.clickStyle_)
                {
                    case (ClickStyle.SELECTION):
                        {
                            break;
                        }
                    case (ClickStyle.IMAGE):
                        {
                            PlaylistBox.Items.Clear();

                            var items = WrapBox.ItemsSource;
                            foreach (var title in items)
                                PlaylistBox.Items.Add(title);
                            PlaylistBox.SelectedIndex = WrapBox.SelectedIndex;
                            break;
                        }
                    case (ClickStyle.VIDEO):
                        {
                            PlaylistBox.Items.Clear();
                            PlaylistBox.Items.Add(WrapBox.SelectedItem);
                            PlaylistBox.SelectedIndex = 0;
                            break;
                        }
                    default:
                        break;
                }
            }
        }

        private void WrapBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (WrapBox.SelectedItems.Count > 0 && clickStyle_ != ClickStyle.MUSIC)
            {
                switch (this.clickStyle_)
                {
                    case (ClickStyle.SELECTION):
                        {
                            break;
                        }
                    case (ClickStyle.IMAGE):
                        {
                            PlaylistBox.Items.Clear();

                            var items = WrapBox.ItemsSource;
                            foreach (var title in items)
                                PlaylistBox.Items.Add(title);
                            PlaylistBox.SelectedIndex = WrapBox.SelectedIndex;
                            this.state_ = State.STOP;
                            MediaPlayer.Stop();
                            ButtonSwitch_Click(sender, e);
                            ButtonPlay_Click(sender, e);

                            break;
                        }
                    case (ClickStyle.VIDEO):
                        {
                            PlaylistBox.Items.Clear();
                            PlaylistBox.Items.Add(WrapBox.SelectedItem);
                            PlaylistBox.SelectedIndex = 0;
                            this.state_ = State.STOP;
                            MediaPlayer.Stop();
                            ButtonSwitch_Click(sender, e);
                            ButtonPlay_Click(sender, e);
                            break;
                        }
                    default:
                        break;
                }
            }
        }

        private void timer_tick_Slide(object sender, EventArgs e)
        {
            if (MediaPlayer.IsVisible)
            {
                Mouse.OverrideCursor = Cursors.None;
                GridControls.Visibility = Visibility.Hidden;
            }
            this.timer_3.Stop();
        }
        private void EventMouseMove(object sender, MouseEventArgs e)
        {
           Point point = Mouse.GetPosition(MediaPlayer);
           Mouse.OverrideCursor = null;
           GridControls.Visibility = Visibility.Visible;
           Console.Out.WriteLine("(y, Y) = "+ point.Y+" , " +MediaPlayer.ActualHeight);
           if ((this.state_ == State.PLAY) && point.Y < (MediaPlayer.ActualHeight - GridControls.ActualHeight))                
                this.timer_3.Start();
           else
               this.timer_3.Stop();
        }

        private void SecondBox_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (PlaylistBox.Items.Count <= 0)
                PlaylistBox.SelectedIndex = 0;
            if (SecondBox.SelectedItems.Count > 0)
                PlaylistBox.Items.Add(SecondBox.SelectedItem);
        }

        private void MainBox_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (MainBox.SelectedItems.Count > 0)
            {
                if (PlaylistBox.Items.Count <= 0)
                    PlaylistBox.SelectedIndex = 0;
                var items = (List<MusicTitle>)MainBox.SelectedItems[0];
                foreach(var title in items)
                    PlaylistBox.Items.Add(title);
            }
        }

        private void WrapBox_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (PlaylistBox.Items.Count <= 0)
                PlaylistBox.SelectedIndex = 0;
            if (clickStyle_ == ClickStyle.IMAGE)
                PlaylistBox.Items.Add(WrapBox.SelectedItem);
            else if (clickStyle_ == ClickStyle.VIDEO)
                PlaylistBox.Items.Add(WrapBox.SelectedItem);
        }

        private void PlaylistBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (PlaylistBox.SelectedItems.Count > 0)
            {
                MediaItem item = (MediaItem)PlaylistBox.SelectedItem;
                this.source_ = item.Path;
                this.MediaPlayer.Source = new Uri(item.Path, UriKind.RelativeOrAbsolute);
                this.state_ = State.STOP;   
                ButtonPlay_Click(sender, e);
            }
        }


        private void DownLoadYoutubeVideo(object sender, RoutedEventArgs e)
        {
            this.sourceVideo_ = this.YoutubeDownload.Text;
            this.YoutubeDownload.Text = "Entrez votre URL...";

        }

        private void PlaylistBox_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (PlaylistBox.Items.Count > 0)
            {
                int current_index = PlaylistBox.SelectedIndex;

                if (current_index == currentIndexLecture_) // si on delete le media en lecture
                {
                    ButtonStop_Click(sender, e);

                }

                if (current_index == PlaylistBox.Items.Count - 1)
                    current_index -= 1;
                PlaylistBox.Items.RemoveAt(PlaylistBox.SelectedIndex);
                if (PlaylistBox.Items.Count > 0)
                    PlaylistBox.SelectedIndex = current_index;
            }
        }
        
    }
}
