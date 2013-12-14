using MWMPV2.classes;
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

namespace WindowsMedia
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    
    public enum State { PLAY, STOP, PAUSE };

    public partial class MainWindow : Window
    {
        private TimeSpan        duree_;
        private String          source_;
        private State           state_;
        private bool            isMuted_;
        private bool            isFullScreen_;
        private DispatcherTimer timer_;
        private double          oldValue;
 
        public MainWindow()
        {
            this.Loaded += MainWindow_Loaded;
            InitializeComponent();
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.state_ = State.STOP;
            this.isMuted_ = false;
            this.isFullScreen_ = false;
            this.oldValue = -1;

            this.MediaPlayer.LoadedBehavior = MediaState.Manual;
            this.MediaPlayer.UnloadedBehavior = MediaState.Manual;

            //source_ = "C:\\Users\\Robert\\Downloads\\destiny.jpg"; //donner un temps aux images
            //this.source_ = "C:\\Users\\Robert\\Downloads\\bestgame.avi";
            this.source_ = "E:\\Disney\\RoiLion.mp3";
            //this.source_ = "C:\\Users\\Stéphane\\Downloads\\lol.mp4";

            this.MediaPlayer.Source = new Uri(this.source_, UriKind.RelativeOrAbsolute);

            this.SliderVolume.Value = 50;

            this.SliderTime.Maximum = this.Width - 160;
            this.SliderTime.Width = this.Width - 160;
            SliderTime.IsMoveToPointEnabled = true;

            var lib = new MusicLibrary(new List<string> { Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) });
            lib.GenerateLibrary();
            MainBox.ItemsSource = lib;

        }

        // Gestion bouton Play/Pause
        private void ButtonPlay_Click(object sender, RoutedEventArgs e)
        {
            ImageBrush brush;

            if (this.state_ == State.STOP || this.state_ == State.PAUSE)
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
                {
                    this.WindowStyle = WindowStyle.SingleBorderWindow;
                    this.WindowState = WindowState.Normal;
                    this.MediaPlayer.Stretch = Stretch.Uniform;
                    this.SliderTime.Width = this.Width - 160;
                    this.SliderTime.Maximum = this.Width - 160;
                    this.isFullScreen_ = false;
                }
                GridBibliotheque.Visibility = System.Windows.Visibility.Visible;
                GridLecture.Visibility = System.Windows.Visibility.Hidden;
            }
        }

        // Gestion du Slide de la video
        void timer_Tick(object sender, EventArgs e)
        {
            double value = (double)((this.MediaPlayer.Position.Hours * 3600) + (this.MediaPlayer.Position.Minutes * 60) + this.MediaPlayer.Position.Seconds) / (double)duree_.TotalSeconds;
            oldValue = value * (double)SliderTime.Width;
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
            double OldPosition = (oldValue * (double)duree_.TotalSeconds) / SliderTime.Width;
            double SliderValue = (double)SliderTime.Value;
            double Position = (SliderValue * (double)duree_.TotalSeconds) / SliderTime.Width;
            if (OldPosition != Position)
                this.MediaPlayer.Position = TimeSpan.FromSeconds(Position);
        }

        // Gestion du FullScreen
        private void EventClicMediaElement(object sender, MouseButtonEventArgs e)
        {
            
            if (this.isFullScreen_ == false && e.ClickCount == 2)
            {
                this.WindowStyle = WindowStyle.None;
                this.WindowState = WindowState.Maximized;
                this.MediaPlayer.Stretch = Stretch.Fill;
                this.SliderTime.Width = this.Width - 160;
                this.SliderTime.Maximum = this.Width - 160;
                this.isFullScreen_ = true;
            }
            else if (this.isFullScreen_ == true && e.ClickCount == 2)
            {
                this.WindowStyle = WindowStyle.SingleBorderWindow;
                this.WindowState = WindowState.Normal;
                this.MediaPlayer.Stretch = Stretch.Uniform;
                this.SliderTime.Width = this.Width - 160;
                this.SliderTime.Maximum = this.Width - 160;
                this.isFullScreen_ = false;
            }
        }

        // Gestion de la modification de la MainWindow
        private void MainWindowUpdated(object sender, EventArgs e)
        {
            this.SliderTime.Width = this.Width - 160;
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
            else if (value >= 1 && value < 34)
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

        private void SecondBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MusicTitle ti = (MusicTitle)e.AddedItems[0];
            this.MediaPlayer.Source = new Uri(ti.Path, UriKind.RelativeOrAbsolute);
            this.source_ = ti.Path;
        }

        private void MainBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MusicAlbum al = (MusicAlbum)e.AddedItems[0];
            SecondBox.ItemsSource = al;
        }
    }
}
