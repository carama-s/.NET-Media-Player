using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WindowsMedia.classes
{
    public class MovieFile : MediaItem, System.ComponentModel.INotifyPropertyChanged
    {
        static private Mutex GenerationMutex = new Mutex(false);
        static public Uri DefaultImagePath = new Uri("../assets/defaultvideoart.png", UriKind.Relative);
        public String Description { get; private set; }
        private BitmapImage GeneratedImage { get; set; }

        protected override BitmapImage GetImage()
        {
            if (GeneratedImage == null)
                return new BitmapImage(DefaultImagePath);
            else
                return GeneratedImage;
        }

        public void BackgroundGenerateImage(object param)
        {
            GenerationMutex.WaitOne();
            MediaPlayer player = new MediaPlayer { Volume = 0, ScrubbingEnabled = true };
            player.Open(new Uri(Path, UriKind.Relative));
            while (player.NaturalDuration.HasTimeSpan == false);
            player.Position = TimeSpan.FromSeconds(player.NaturalDuration.TimeSpan.TotalSeconds / 2);
            System.Threading.Thread.Sleep(1000);




            RenderTargetBitmap rtb = new RenderTargetBitmap(320, 180, 96, 96, PixelFormats.Pbgra32);
            DrawingVisual dv = new DrawingVisual();
            DrawingContext dc = dv.RenderOpen();
            dc.DrawVideo(player, new Rect(0, 0, 320, 240));
            dc.Close();
            rtb.Render(dv);

            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            MemoryStream memoryStream = new MemoryStream();
            GeneratedImage = new BitmapImage();
            encoder.Frames.Add(BitmapFrame.Create(rtb));
            encoder.Save(memoryStream);
            GeneratedImage.BeginInit();
            GeneratedImage.StreamSource = new MemoryStream(memoryStream.ToArray());
            GeneratedImage.EndInit();
            GeneratedImage.Freeze();
            memoryStream.Close();
            player.Close();
            OnPropertyChanged("Image");
            GenerationMutex.ReleaseMutex();
        }

        public MovieFile(String path)
        {
            GeneratedImage = null;
            var tags = TagLib.File.Create(path);
            Path = path;
            Artist = "";
            Duration = tags.Properties.Duration;
            Title = tags.Name.Substring(0,tags.Name.LastIndexOf('.')).Split("\\".ToCharArray()).Last();
            Type = ClickStyle.VIDEO;
            MessageColor = Colors.White;
            ThreadPool.QueueUserWorkItem(BackgroundGenerateImage, null);
        }

        public MovieFile()
        {
        }

        public override Object Clone()
        {
            return new MovieFile { Path = Path,
                                   Artist = Artist,
                                   Duration = Duration,
                                   Title = Title,
                                   Type = Type,
                                   MessageColor = Colors.White,
                                   GeneratedImage = GeneratedImage };
        }
    }
}
