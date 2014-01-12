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
    public enum ClickStyle { SELECTION, MUSIC, IMAGE, VIDEO, ALL};

    public partial class MainWindow : MetroWindow
    {
        private TimeSpan duree_;
        private String source_;
        private State state_;
        public MusicStyle musicStyle_;
        public ClickStyle clickStyle_;
        private bool isMuted_;
        private bool isFullScreen_;
        private bool isRepeat_;
        private bool isShuffle_;
        private DispatcherTimer timer_Slide;
        private DispatcherTimer timer_EventMouse;
        private double oldValue;
        delegate void DelegateSource(MainWindow win);
        private double oldSize_;
        public Library lib_;
        private bool isInit;

        private int currentIndexLecture_;


        public MainWindow()
        {
            this.oldSize_ = -1;
            this.Loaded += MainWindow_Loaded;
            AddHandler(Keyboard.KeyDownEvent, (KeyEventHandler)WindowKeyDown);

            InitializeComponent();

            this.Width = ConfigFile.Instance.Data.Width;
            this.Height = ConfigFile.Instance.Data.Height;
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.isInit = false;

            this.timer_Slide = new DispatcherTimer();
            this.timer_Slide.Interval = TimeSpan.FromMilliseconds(100);
            this.timer_Slide.Tick += new EventHandler(timer_Tick);

            this.timer_EventMouse = new DispatcherTimer();
            this.timer_EventMouse.Interval = TimeSpan.FromSeconds(2);
            this.timer_EventMouse.Tick += new EventHandler(timer_tick_Slide);

            this.isFullScreen_ = false;
            LoadConfigProperty();
            this.oldValue = -1;
            this.state_ = State.STOP;
            this.clickStyle_ = ClickStyle.MUSIC;
            this.MediaPlayer.LoadedBehavior = MediaState.Manual;
            this.MediaPlayer.UnloadedBehavior = MediaState.Manual;

            this.SliderTime.Maximum = this.Width - 160;
            this.SliderTime.IsMoveToPointEnabled = true;

            this.currentIndexLecture_ = -1;

            lib_ = new Library(this);
            lib_.GenerateLibrary();
            BoxSelectMedia_SelectionChanged(sender, null);
            this.isInit = true;
        }

        public void UpdateCurrentPanel(ClickStyle currentStyle)
        {
            Action action = delegate
            {
                if (currentStyle == clickStyle_ || currentStyle == ClickStyle.ALL)
                {
                    SecondBox.ItemsSource = null;
                    if (clickStyle_ == ClickStyle.MUSIC)
                    {
                        if (musicStyle_ == MusicStyle.ALBUM)
                            MainBox.ItemsSource = new AlbumIterator(lib_);
                        else if (musicStyle_ == MusicStyle.ARTIST)
                            MainBox.ItemsSource = new ArtistIterator(lib_);
                        else if (musicStyle_ == MusicStyle.GENRE)
                            MainBox.ItemsSource = new GenreIterator(lib_);
                    }
                    else if (clickStyle_ == ClickStyle.IMAGE)
                        WrapBox.ItemsSource = new ImageIterator(lib_);
                    else if (clickStyle_ == ClickStyle.VIDEO)
                        WrapBox.ItemsSource = new MovieIterator(lib_);
                    else if (clickStyle_ == ClickStyle.SELECTION)
                    {
                        List<Playlist> list = new List<Playlist>();
                        foreach (var item in this.lib_.Playlists)
                            list.Add((Playlist)item);
                        this.MainBox.ItemsSource = list;
                    }
                }

            };
            Dispatcher.Invoke(action);
        }

        private void LoadConfigProperty()
        {
            ImageBrush brush;

            // Volume (bouton + slider)

            this.SliderVolume.Value = ConfigFile.Instance.Data.Volume;
            this.isMuted_ = ConfigFile.Instance.Data.Muted;

            brush = createBrushVolume(this.SliderVolume.Value);
            if (this.isMuted_)
            {
                brush = createBrushVolume(0);
                this.MediaPlayer.Volume = 0;
            }
            this.ButtonVolume.Background = brush;
            this.ButtonVolume.OpacityMask = brush;

            // Bouton Repeat

            this.isRepeat_ = ConfigFile.Instance.Data.Repeat;
            if (this.isRepeat_)
            {
                this.ButtonRepeat.Background = DefaultImageGetter.Instance.EnableRepeat;
                this.ButtonRepeat.OpacityMask = DefaultImageGetter.Instance.EnableRepeat;
            }

            // Bouton Shuffle

            this.isShuffle_ = ConfigFile.Instance.Data.Shuffle;
            if (this.isShuffle_)
            {
                this.ButtonShuffle.Background = DefaultImageGetter.Instance.EnableShuffle;
                this.ButtonShuffle.OpacityMask = DefaultImageGetter.Instance.EnableShuffle;
            }

            // ClickMusicStyle

            BrushConverter bc = new BrushConverter();
            this.musicStyle_ = ConfigFile.Instance.Data.Style;
            if (musicStyle_ == MusicStyle.ALBUM)
                this.ButtonAlbums.Foreground = (Brush)bc.ConvertFrom("#FF41B1E1");
            else if (musicStyle_ == MusicStyle.ARTIST)
                this.ButtonArtists.Foreground = (Brush)bc.ConvertFrom("#FF41B1E1");
            else
                this.ButtonGenres.Foreground = (Brush)bc.ConvertFrom("#FF41B1E1");

            // WindowsState

            this.WindowState = ConfigFile.Instance.Data.WinState;
        }

        private void WindowKeyDown(object sender, KeyEventArgs e)
        {
            if (isFullScreen_ == true && e.Key == Key.Escape)
                FullScreenOff();
            else if ((e.Key == Key.O) && Keyboard.IsKeyDown(Key.LeftCtrl))
                PlayItem(sender, e);
            else if ((e.Key == Key.B) && Keyboard.IsKeyDown(Key.LeftCtrl))
                OpenBiblioWindow(sender, e);
            else if ((e.Key == Key.F5))
                RefreshLib(sender, e);
        }

        // Gestion bouton Play/Pause
        private void ButtonPlay_Click(object sender, RoutedEventArgs e)
        {
            ImageBrush brush;

            if ((this.state_ == State.STOP || this.state_ == State.PAUSE))
            {
                if (PlaylistBox.Items.Count == 0 && this.state_ != State.PAUSE)
                {
                    if (clickStyle_ == ClickStyle.MUSIC || clickStyle_ == ClickStyle.SELECTION)
                    {
                        if (MainBox.SelectedItems.Count > 0 && SecondBox.SelectedItems.Count <= 0)
                            FillPlaylistMainBox();
                        else if (MainBox.SelectedItems.Count > 0 && SecondBox.SelectedItems.Count > 0)
                            FillPlaylistSecondBox();
                    }
                    else if (clickStyle_ == ClickStyle.IMAGE || clickStyle_ == ClickStyle.VIDEO)
                        if (WrapBox.SelectedItems.Count > 0)
                        {
                            FillPlaylistWrapBox();
                            ButtonSwitch_Click(sender, e);
                        }
                }

                if (PlaylistBox.Items.Count > 0 && this.state_ != State.PAUSE)
                {
                    MediaItem item = (MediaItem)PlaylistBox.Items[currentIndexLecture_];
                    this.source_ = item.Path;
                    this.MediaPlayer.Source = new Uri(item.Path, UriKind.RelativeOrAbsolute);

                    if (item.Type == ClickStyle.MUSIC)
                    {
                        this.LectureMusicImage.Source = item.Image;
                        this.LectureMusicImage.Visibility = System.Windows.Visibility.Visible;
                    }
                    else
                        this.LectureMusicImage.Visibility = System.Windows.Visibility.Hidden;
                }

                if (this.source_ != null)
                {
                    brush = createBrush("assets/icon-pause-barre.png");
                    this.state_ = State.PLAY;
                    this.MediaPlayer.Play();
                    if (oldValue == -1)
                    {
                        this.CurrentTime.Text = "00:00:00";
                        this.SliderTime.Value = 0;
                    }
                    String[] media = this.source_.Split('\\');
                    MediaItem item = (MediaItem)PlaylistBox.Items[currentIndexLecture_];
                    this.duree_ = item.Duration;
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
                this.state_ = State.STOP;
                this.SliderTime.Value = 0;
                this.oldValue = -1;
                this.MediaPlayer.Stop();
                Mouse.OverrideCursor = null;
                this.MediaPlayer.Visibility = Visibility.Hidden;
                this.LectureMusicImage.Visibility = Visibility.Hidden;
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
                    this.FullScreenOff();
                GridBibliotheque.Visibility = System.Windows.Visibility.Visible;
                GridLecture.Visibility = System.Windows.Visibility.Hidden;
            }
        }

        // Gestion du bouton Back
        private void BackButtonMediaElement(object sender, RoutedEventArgs e)
        {
            if (PlaylistBox.Items.Count > 0)
            {
                if (isShuffle_)
                {
                    ShuffleList();
                    return;
                }
                ResetIndexLecture();

                if (currentIndexLecture_ == 0)
                    currentIndexLecture_ = PlaylistBox.Items.Count - 1;
                else
                    currentIndexLecture_ -= 1;

                SelectIndexLecture(currentIndexLecture_);

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
                if (isShuffle_)
                {
                    ShuffleList();
                    return;
                }
                ResetIndexLecture();
                if (currentIndexLecture_ == (PlaylistBox.Items.Count - 1))
                    currentIndexLecture_ = 0;
                else
                    currentIndexLecture_ += 1;

                SelectIndexLecture(currentIndexLecture_);

                this.source_ = null;
                this.state_ = State.STOP;
                this.ButtonPlay_Click(sender, e);
            }
        }

        // Gestion du Slide de la video
        void timer_Tick(object sender, EventArgs e)
        {
            if (currentIndexLecture_ >= 0 && ((MediaItem)this.PlaylistBox.Items[currentIndexLecture_]).Type != ClickStyle.IMAGE && this.duree_.TotalSeconds > 0)
            {
                double value = (double)((this.MediaPlayer.Position.Hours * 3600) + (this.MediaPlayer.Position.Minutes * 60) + this.MediaPlayer.Position.Seconds) / (double)this.duree_.TotalSeconds;
                oldValue = value * (double)this.SliderTime.Maximum;
                this.SliderTime.Value = oldValue;
                this.CurrentTime.Text = this.MediaPlayer.Position.ToString();
            }
            else
            {
                this.CurrentTime.Text = "00:00:00";
                this.SliderTime.Value = 0;
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
            this.IgnoreTaskbarOnMaximize = true;
            this.ShowTitleBar = false;
            this.WindowStyle = WindowStyle.None;
            if (this.WindowState == WindowState.Maximized)
                this.WindowState = System.Windows.WindowState.Normal;
            this.WindowState = WindowState.Maximized;
            this.MediaPlayer.Stretch = Stretch.Uniform;
            this.SliderTime.Maximum = this.Width - 160;
            this.isFullScreen_ = true;
        }

        private void FullScreenOff()
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
                this.FullScreenOff();
        }

        private void EventClicLectureImage(object sender, MouseButtonEventArgs e)
        {
            if (this.isFullScreen_ == false && e.ClickCount == 2)
                this.FullScreenOn();
            else if (this.isFullScreen_ == true && e.ClickCount == 2)
                this.FullScreenOff();
        }

        // Gestion de la modification de la MainWindow
        private void MainWindowUpdated(object sender, EventArgs e)
        {
            this.oldSize_ = (double)this.SliderTime.Maximum;
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

            if (value == 0)
                brush = createBrush("assets/icon-volumemute-barre.png");
            else if (value >= 0.1 && value < 34)
                brush = createBrush("assets/icon-volume1-barre.png");
            else if (value >= 34 && value <= 67)
                brush = createBrush("assets/icon-volume2-barre.png");
            else
                brush = createBrush("assets/icon-volumemax-barre.png");
            return (brush);
        }

        private void ResetIndexLecture()
        {
            if (currentIndexLecture_ >= 0)
            {
                MediaItem obj = (MediaItem)PlaylistBox.Items[currentIndexLecture_];
                obj.MessageColor = Colors.White;
            }
        }

        private void SelectIndexLecture(int index)
        {
            currentIndexLecture_ = index;
            if (currentIndexLecture_ >= 0)
            {
                MediaItem obj = (MediaItem)PlaylistBox.Items[currentIndexLecture_];
                obj.MessageColor = Color.FromRgb(65, 177, 225);

                this.PlayingItemImage.Source = obj.Image;
                this.PlayingItemTitle.Text = obj.Title;
                this.PlayingItemArtist.Text = obj.Artist;
            }
            else
            {
                this.PlayingItemImage.Source = new BitmapImage();
                this.PlayingItemTitle.Text = "";
                this.PlayingItemArtist.Text = "";
            }
        }

        // Gestion fin de Media Element
        private void EventEndMedia(object sender, RoutedEventArgs e)
        {
            this.ButtonStop_Click(sender, e);
            this.GridControls.Visibility = Visibility.Visible;

            if (isShuffle_)
                ShuffleList();
            else if (currentIndexLecture_ < (PlaylistBox.Items.Count - 1))
                ForwardButtonMediaElement(sender, e);
            else if (isRepeat_)
                ForwardButtonMediaElement(sender, e);
            else
            {
                ResetIndexLecture();
                SelectIndexLecture(0);

                this.source_ = null;
                this.state_ = State.STOP;
            }

        }

        // Gestion Box bibliothèque
        private void MainBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                switch (this.clickStyle_)
                {
                    case (ClickStyle.SELECTION):
                        {
                            var al = (Playlist)e.AddedItems[0];
                            SecondBox.ItemsSource = al.Medias;
                            break;
                        }
                    case (ClickStyle.MUSIC):
                        {
                            if (musicStyle_ == MusicStyle.ALBUM)
                            {
                                var al = (List<MusicTitle>)e.AddedItems[0];
                                SecondBox.ItemsSource = al;
                            }
                            break;
                        }
                    default:
                        break;
                }
            }
        }

        private void FillPlaylistMainBox()
        {
            ResetIndexLecture();
            PlaylistBox.Items.Clear();

            if (clickStyle_ == ClickStyle.MUSIC)
            {
                var itemsMusic = (List<MusicTitle>)MainBox.SelectedItems[0];
                foreach (var title in itemsMusic)
                    PlaylistBox.Items.Add(title);
            }
            else
            {
                var item = (Playlist)MainBox.SelectedItems[0];
                var itemsSelection = item.Medias;
                foreach (var title in itemsSelection)
                    PlaylistBox.Items.Add(title);
            }

            this.state_ = State.STOP;
            SelectIndexLecture(0);
            PlaylistBox_SourceUpdated();
        }

        private void MainBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (MainBox.SelectedItems.Count > 0 && e.ChangedButton == MouseButton.Left)
            {
                if (musicStyle_ != MusicStyle.ALBUM)
                {
                    var al = (MusicArtist)MainBox.SelectedItem;
                    MainBox.ItemsSource = al;
                    musicStyle_ = MusicStyle.ALBUM;
                    MainBox.ItemTemplate = FindResource("MainAlbumTemplate") as DataTemplate;
                }
                else
                {
                    FillPlaylistMainBox();
                    ButtonPlay_Click(sender, e);
                }
            }
        }

        private void MainBox_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (MainBox.SelectedItems.Count > 0)
            {
                if (clickStyle_ == ClickStyle.MUSIC && musicStyle_ != MusicStyle.ALBUM)
                    return;
                bool WasEmpty = true;

                if (PlaylistBox.Items.Count > 0)
                    WasEmpty = false;
                if (clickStyle_ == ClickStyle.MUSIC)
                {
                    var itemsMusic = (List<MusicTitle>)MainBox.SelectedItems[0];
                    foreach (var title in itemsMusic)
                        PlaylistBox.Items.Add(title.Clone());
                }
                else
                {
                    var item = (Playlist)MainBox.SelectedItems[0];
                    var itemsSelection = item.Medias;
                    foreach (var title in itemsSelection)
                        PlaylistBox.Items.Add(title.Clone());
                }

                if (WasEmpty)
                {
                    ResetIndexLecture();
                    SelectIndexLecture(0);
                }
                PlaylistBox_SourceUpdated();
            }
        }

        private void FillPlaylistSecondBox()
        {
            ResetIndexLecture();
            PlaylistBox.Items.Clear();

            if (clickStyle_ == ClickStyle.MUSIC)
            {
                var itemsMusic = (List<MusicTitle>)MainBox.SelectedItems[0];
                foreach (var title in itemsMusic)
                    PlaylistBox.Items.Add(title);
            }
            else
            {
                var item = (Playlist)MainBox.SelectedItems[0];
                var itemsSelection = item.Medias;
                foreach (var title in itemsSelection)
                    PlaylistBox.Items.Add(title.Clone());
            }

            SelectIndexLecture(SecondBox.SelectedIndex);
            this.state_ = State.STOP;
            PlaylistBox_SourceUpdated();
        }

        private void SecondBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (SecondBox.SelectedItems.Count > 0 && e.ChangedButton == MouseButton.Left)
            {
                FillPlaylistSecondBox();
                ButtonPlay_Click(sender, e);
            }
        }

        private void SecondBox_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (SecondBox.SelectedItems.Count > 0)
            {
                bool WasEmpty = true;

                if (PlaylistBox.Items.Count > 0)
                    WasEmpty = false;
                if (clickStyle_ == ClickStyle.MUSIC || clickStyle_ == ClickStyle.SELECTION)
                    PlaylistBox.Items.Add(((MediaItem)SecondBox.SelectedItem).Clone());
                if (WasEmpty)
                {
                    ResetIndexLecture();
                    SelectIndexLecture(0);
                }
                PlaylistBox_SourceUpdated();
            }
        }

        private void BoxSelectMedia_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.isInit == true)
                this.TextBoxSearch.Text = "";

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
            if (this.isRepeat_)
            {
                this.isRepeat_ = false;
                this.ButtonRepeat.Background = DefaultImageGetter.Instance.DisabledRepeat;
                this.ButtonRepeat.OpacityMask = DefaultImageGetter.Instance.DisabledRepeat;
            }
            else
            {
                this.isRepeat_ = true;
                this.ButtonRepeat.Background = DefaultImageGetter.Instance.EnableRepeat;
                this.ButtonRepeat.OpacityMask = DefaultImageGetter.Instance.EnableRepeat;
            }
        }

        private void ButtonShuffle_Click(object sender, RoutedEventArgs e)
        {
            ImageBrush brush;

            if (this.isShuffle_)
            {
                this.isShuffle_ = false;
                this.ButtonShuffle.Background = DefaultImageGetter.Instance.DisabledShuffle;
                this.ButtonShuffle.OpacityMask = DefaultImageGetter.Instance.DisabledShuffle;
            }
            else
            {
                this.isShuffle_ = true;
                this.ButtonShuffle.Background = DefaultImageGetter.Instance.EnableShuffle;
                this.ButtonShuffle.OpacityMask = DefaultImageGetter.Instance.EnableShuffle; ;
            }
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
            dlg.Filter = "Media files | *.mp3; .flac; .m4a; *.mp4; *.avi; *.wmv; *.jpeg; *.png; *.jpg; *.bmp";
            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                this.source_ = dlg.FileName;

                MediaItem item = MediaItem.Create(dlg.FileName);
                if (item != null)
                {
                    PlaylistBox.Items.Clear();
                    PlaylistBox.Items.Add(item);
                    ResetIndexLecture();
                    SelectIndexLecture(0);
                    this.state_ = State.STOP;
                    PlaylistBox_SourceUpdated();
                MediaPlayer.Stop();
                this.state_ = State.STOP;
                this.ButtonPlay_Click(sender, e);
            }
        }
        }


        private void OpenBiblioWindow(object sender, RoutedEventArgs e)
        {
            var newwindow = new BiblioWindow(this);
            newwindow.Owner = this;
            newwindow.ShowDialog();
        }

        public void RefreshLib(object sender, RoutedEventArgs e)
        {
            lib_.GenerateLibrary();
            BoxSelectMedia_SelectionChanged(sender, null);
        }

        private void Quit(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void FillPlaylistWrapBox()
        {
            ResetIndexLecture();
            PlaylistBox.Items.Clear();

            switch (this.clickStyle_)
            {
                case (ClickStyle.IMAGE):
                    {
                        var items = WrapBox.ItemsSource;
                        foreach (var title in items)
                            PlaylistBox.Items.Add(title);
                        SelectIndexLecture(WrapBox.SelectedIndex);
                        break;
                    }
                case (ClickStyle.VIDEO):
                    {
                        PlaylistBox.Items.Add(WrapBox.SelectedItem);
                        SelectIndexLecture(0);
                        break;
                    }
                default:
                    break;
            }
            this.state_ = State.STOP;
            MediaPlayer.Stop();
        }

        private void WrapBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && WrapBox.SelectedItems.Count > 0 && (clickStyle_ != ClickStyle.MUSIC && clickStyle_ != ClickStyle.SELECTION))
            {
                FillPlaylistWrapBox();
                ButtonPlay_Click(sender, e);
                ButtonSwitch_Click(sender, e);
                PlaylistBox_SourceUpdated();
            }
        }

        private void WrapBox_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (WrapBox.SelectedItems.Count > 0)
            {
                bool WasEmpty = true;

                if (PlaylistBox.Items.Count > 0)
                    WasEmpty = false;
                if (clickStyle_ == ClickStyle.IMAGE || clickStyle_ == ClickStyle.VIDEO)
                    PlaylistBox.Items.Add(((MediaItem)WrapBox.SelectedItem).Clone());
                if (WasEmpty)
                {
                    ResetIndexLecture();
                    SelectIndexLecture(0);
                }

                PlaylistBox_SourceUpdated();
            }
        }

        private void timer_tick_Slide(object sender, EventArgs e)
        {
            if (MediaPlayer.IsVisible)
            {
                Mouse.OverrideCursor = Cursors.None;
                GridControls.Visibility = Visibility.Hidden;
            }
            this.timer_EventMouse.Stop();
        }
        private void EventMouseMove(object sender, MouseEventArgs e)
        {
            Point point = Mouse.GetPosition(MediaPlayer);
            Mouse.OverrideCursor = null;
            GridControls.Visibility = Visibility.Visible;
            Console.Out.WriteLine(point.Y);
            if ((this.state_ == State.PLAY) && (point.Y > 10 && point.Y < (MediaPlayer.ActualHeight - GridControls.ActualHeight)))
                this.timer_EventMouse.Start();
            else
                this.timer_EventMouse.Stop();
        }

        private void LeaveMainWindow(object sender, MouseEventArgs e)
        {
            this.timer_EventMouse.Stop();
            GridControls.Visibility = Visibility.Visible;
        }

        private void PlaylistBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && PlaylistBox.SelectedItems.Count > 0)
            {
                ResetIndexLecture();
                SelectIndexLecture(PlaylistBox.SelectedIndex);

                MediaItem item = (MediaItem)PlaylistBox.Items[currentIndexLecture_];
                this.source_ = item.Path;
                this.MediaPlayer.Source = new Uri(item.Path, UriKind.RelativeOrAbsolute);
                this.state_ = State.STOP;
                ButtonPlay_Click(sender, e);
            }
        }

        private void PlaylistBox_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (PlaylistBox.SelectedIndex > -1 && PlaylistBox.Items.Count > 0)
            {
                ResetIndexLecture();

                if (PlaylistBox.SelectedIndex == currentIndexLecture_) // si on delete le media en lecture
                {
                    if (PlaylistBox.SelectedIndex == PlaylistBox.Items.Count - 1) // et si c'est le dernier
                        currentIndexLecture_ -= 1;
                    ButtonStop_Click(sender, e);
                    this.source_ = null;
                }
                else if (PlaylistBox.SelectedIndex < currentIndexLecture_ && PlaylistBox.SelectedIndex < PlaylistBox.Items.Count)
                    currentIndexLecture_ -= 1;

                if (PlaylistBox.SelectedIndex < PlaylistBox.Items.Count)
                    PlaylistBox.Items.RemoveAt(PlaylistBox.SelectedIndex);

                SelectIndexLecture(currentIndexLecture_);

                PlaylistBox_SourceUpdated();
            }
        }

        private void PlaylistBox_SourceUpdated()
        {
            var list = PlaylistBox.Items;
            TimeSpan total = new TimeSpan();
            foreach (var item in list)
            {
                var temp = (MediaItem)item;
                total += temp.Duration;
            }
            DurationBox.Text = String.Format("{0:d2}:{1:d2}:{2:d2}", (int)total.TotalHours, total.Minutes, total.Seconds);
        }

        private void ButtonSupprimer_Click(object sender, RoutedEventArgs e)
        {
            if (clickStyle_ == ClickStyle.SELECTION && MainBox.SelectedItems.Count > 0)
            {
                var newwindow = new DeleteWindow(this);
                newwindow.Owner = this;
                newwindow.ShowDialog();
            }
        }
        private void ButtonRenommer_Click(object sender, RoutedEventArgs e)
        {
            if (clickStyle_ == ClickStyle.SELECTION && MainBox.SelectedItems.Count > 0)
            {
                var newwindow = new RenameWindow(this);
                newwindow.Owner = this;
                newwindow.ShowDialog();
            }
        }

        private void ButtonCreer_Click(object sender, RoutedEventArgs e)
        {
            if (clickStyle_ == ClickStyle.SELECTION)
            {
                var newwindow = new AddWindow(this);
                newwindow.Owner = this;
                newwindow.ShowDialog();
            }
        }

        private void ShuffleList()
        {
            if (PlaylistBox.Items.Count > 1)
            {
                Random rd = new Random();
                int ran = rd.Next(PlaylistBox.Items.Count);
                while (ran == currentIndexLecture_)
                    ran = rd.Next(PlaylistBox.Items.Count);
                ResetIndexLecture();
                SelectIndexLecture(ran);
                this.state_ = State.STOP;
                ButtonPlay_Click(null, null);
            }
        }

        private void MetroWindow_Closed(object sender, EventArgs e)
        {
            ConfigFile.Instance.Data.Height = (int)this.Height;
            ConfigFile.Instance.Data.Width = (int)this.Width;
            ConfigFile.Instance.Data.Muted = this.isMuted_;
            ConfigFile.Instance.Data.Repeat = this.isRepeat_;
            ConfigFile.Instance.Data.Shuffle = this.isShuffle_;
            ConfigFile.Instance.Data.Volume = (int)this.SliderVolume.Value;
            ConfigFile.Instance.Data.Style = this.musicStyle_;
            ConfigFile.Instance.Data.WinState = this.WindowState;

            if (this.WindowState == System.Windows.WindowState.Maximized)
            {
                ConfigFile.Instance.Data.Height = 600;
                ConfigFile.Instance.Data.Width = 800;
            }
            ConfigFile.Instance.Write();
        }

        private void TextBoxSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (clickStyle_ == ClickStyle.MUSIC)
            {
                this.SecondBox.ItemsSource = null;
                if (TextBoxSearch.Text != "")
                {
                    if (this.MainBox.Visibility == System.Windows.Visibility.Visible)
                    {
                        this.SecondBox.Visibility = System.Windows.Visibility.Hidden;
                        this.MainBox.Visibility = System.Windows.Visibility.Hidden;
                        this.BoxSearchMusic.Visibility = System.Windows.Visibility.Visible;
                    }
                    this.BoxSearchMusic.ItemsSource = new MusicSearchIterator(lib_, TextBoxSearch.Text);
                    this.BoxSearchMusic.SelectedIndex = -1;                   
                }
                else
                {
                    this.SecondBox.Visibility = System.Windows.Visibility.Visible;
                    this.MainBox.Visibility = System.Windows.Visibility.Visible;
                    this.BoxSearchMusic.Visibility = System.Windows.Visibility.Hidden;
                    if (musicStyle_ == MusicStyle.ALBUM)
                        this.MainBox.ItemsSource = new AlbumIterator(lib_);
                    else if (musicStyle_ == MusicStyle.ARTIST)
                        this.MainBox.ItemsSource = new ArtistIterator(lib_);
                    else
                        this.MainBox.ItemsSource = new GenreIterator(lib_);
                }
            }
            else if (clickStyle_ == ClickStyle.IMAGE)
            {
                if (TextBoxSearch.Text != "")
                {
                    this.SecondBox.ItemsSource = null;
                    this.MainBox.ItemsSource = null;
                    this.WrapBox.ItemsSource = new ImageSearchIterator(lib_, TextBoxSearch.Text);
                }
                else
                    this.WrapBox.ItemsSource = new ImageIterator(lib_);
            }
            else if (clickStyle_ == ClickStyle.VIDEO)
            {
                if (TextBoxSearch.Text != "")
                {
                    this.SecondBox.ItemsSource = null;
                    this.MainBox.ItemsSource = null;
                    this.WrapBox.ItemsSource = new MovieSearchIterator(lib_, TextBoxSearch.Text);
                }
                else
                    this.WrapBox.ItemsSource = new MovieIterator(lib_);
            }
        }

        private void LoupeImage_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            TextBoxSearch.Text = "";
        }

        private void BoxSearchMusic_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && BoxSearchMusic.SelectedItems.Count > 0)
            {
                ResetIndexLecture();

                PlaylistBox.Items.Clear();
                PlaylistBox.Items.Add(((MediaItem)BoxSearchMusic.SelectedItem).Clone());
                if (PlaylistBox.Items.Count > 0)
                    SelectIndexLecture(0);
                PlaylistBox_SourceUpdated();
                this.state_ = State.STOP;
                ButtonPlay_Click(sender, e);
            }
        }

        private void BoxSearchMusic_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            Console.WriteLine("click");

            if (BoxSearchMusic.SelectedItems.Count > 0)
            {
                Console.WriteLine("lol");
                bool WasEmpty = true;

                if (PlaylistBox.Items.Count > 0)
                    WasEmpty = false;
                PlaylistBox.Items.Add(((MediaItem)BoxSearchMusic.SelectedItem).Clone());
                if (WasEmpty)
                {
                    ResetIndexLecture();
                    SelectIndexLecture(0);
                }

                PlaylistBox_SourceUpdated();
            }
        }
    }
}

