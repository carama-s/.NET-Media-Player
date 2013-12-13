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

namespace WindowsMedia
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    /// 
    
    public enum State { PLAY, STOP, PAUSE };

    public partial class MainWindow : Window
    {
        public TimeSpan duree_;
        public String   source_;
        public State    state_;
        public bool     isMuted_;
        public bool     isFullScreen_;
         
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

            MediaPlayer.LoadedBehavior = MediaState.Manual;
            MediaPlayer.UnloadedBehavior = MediaState.Manual;

            //source_ = "C:\\Users\\Robert\\Downloads\\destiny.jpg";
            source_ = "C:\\Users\\Robert\\Downloads\\bestgame.avi";
            //source_ = "E:\\HappinessTherapy.mkv";
            //source_ = "C:\\Users\\Robert\\Music\\Assassin's Creed 4 Black Flag Original Soundtrack MP3 V0 Transcode\\04. The High Seas.mp3";
            //this.source_ = "C:\\Users\\Stéphane\\Downloads\\lol.mp4";
            
            MediaPlayer.Source = new Uri(this.source_, UriKind.RelativeOrAbsolute);

            this.SliderVolume.Value = 50;

            this.SliderVolume.Value = 50;
            this.SliderTime.Width = this.Width;
            SliderTime.IsMoveToPointEnabled = true;

            List<TreeMenuTemplateClass> root = new List<TreeMenuTemplateClass>();
            root.Add(new TreeMenuTemplateClass("Sélections", ""));
            TreeMenuTemplateClass music = new TreeMenuTemplateClass("Musiques", "");
            music.SubMenu.Add(new TreeSubMenuTemplateClass("Interprète", ""));
            music.SubMenu.Add(new TreeSubMenuTemplateClass("Album", ""));
            music.SubMenu.Add(new TreeSubMenuTemplateClass("Genre", ""));
            root.Add(music);
            root.Add(new TreeMenuTemplateClass("Vidéos", ""));
            root.Add(new TreeMenuTemplateClass("Images", ""));
            /*
            MenuTreeView.ItemsSource = root;
            var lib = new MusicLibrary(new List<string> { Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) });
            lib.GenerateLibrary();
            ListViewContent.ItemsSource = lib;
            */

        }

        private void ButtonPlay_Click(object sender, RoutedEventArgs e)
        {
            ImageBrush brush;

            if (this.state_ == State.STOP || this.state_ == State.PAUSE)
            {
                brush = createBrush("assets/icon-pause-barre.png");
                this.state_ = State.PLAY;
                MediaPlayer.Play();
                var tags = TagLib.File.Create(this.source_);
                this.duree_ = tags.Properties.Duration;
            }
            else // this.state_ == State.PLAY
            {
                brush = createBrush("assets/icon-play-barre.png");
                this.state_ = State.PAUSE;
                MediaPlayer.Pause();
            }
            this.ButtonPlay.Background = brush;
            this.ButtonPlay.OpacityMask = brush;
        }

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

        private void ButtonStop_Click(object sender, RoutedEventArgs e)
        {
            if (this.state_ == State.PAUSE || this.state_ == State.PLAY)
            {
                ImageBrush brush = createBrush("assets/icon-play-barre.png");
                this.ButtonPlay.Background = brush;
                this.ButtonPlay.OpacityMask = brush;
                this.state_ = State.STOP;
                MediaPlayer.Stop();
            }
        }

        private void SliderVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ImageBrush brush;

            brush = createBrushVolume(this.SliderVolume.Value);

            this.ButtonVolume.Background = brush;
            this.ButtonVolume.OpacityMask = brush;
            MediaPlayer.Volume = (double)(SliderVolume.Value / 100);
        }

        private void ButtonVolume_Click(object sender, RoutedEventArgs e)
        {
            ImageBrush brush;

            if (this.isMuted_)
            {
                brush = createBrushVolume(this.SliderVolume.Value);
                MediaPlayer.Volume = (double)(SliderVolume.Value / 100);
                this.isMuted_ = false;
            }
            else
            {
                brush = createBrushVolume(0);
                MediaPlayer.Volume = 0;
                this.isMuted_ = true;
            }
            this.ButtonVolume.Background = brush;
            this.ButtonVolume.OpacityMask = brush;
        }

        private void SliderTime_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

            double SliderValue = (double)SliderTime.Value * (SliderTime.Width / 50);
            double Position = (SliderValue * (double)duree_.TotalSeconds) / SliderTime.Width;
            Console.Out.WriteLine(Position);
            MediaPlayer.Position = TimeSpan.FromSeconds(Position);
        }

        private void EventClicMediaElement(object sender, MouseButtonEventArgs e)
        {
            if (this.isFullScreen_ == false && e.ClickCount == 2)
            {
                this.WindowStyle = WindowStyle.None;
                this.WindowState = WindowState.Maximized;
                MediaPlayer.Stretch = Stretch.Fill;
                this.SliderTime.Width = this.Width;
                this.isFullScreen_ = true;
            }
            else if (this.isFullScreen_ == true && e.ClickCount == 2)
            {
                this.WindowStyle = WindowStyle.SingleBorderWindow;
                this.WindowState = WindowState.Normal;
                MediaPlayer.Stretch = Stretch.Uniform;
                this.SliderTime.Width = this.Width;
                this.isFullScreen_ = false;
            }
        }

        private void ButtonSwitch_Click(object sender, RoutedEventArgs e)
        {
            if (GridLecture.Visibility == System.Windows.Visibility.Hidden)
            {
                GridLecture.Visibility = System.Windows.Visibility.Visible;
                GridBibliotheque.Visibility = System.Windows.Visibility.Hidden;
            }
            else
            {
                GridBibliotheque.Visibility = System.Windows.Visibility.Visible;
                GridLecture.Visibility = System.Windows.Visibility.Hidden;
            }
        }
    }
}
