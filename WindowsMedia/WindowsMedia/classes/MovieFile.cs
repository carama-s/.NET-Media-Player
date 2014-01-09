using AsfMojo.Media;
//using DexterLib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
//using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WindowsMedia.classes
{
    public class MovieFile : MediaItem
    {
        public String Description { get; private set; }

        protected override BitmapImage GetImage()
        {         
            MediaPlayer player = new MediaPlayer { Volume = 0, ScrubbingEnabled = true };
            player.Open(new Uri(Path, UriKind.Relative));
            player.Position = TimeSpan.FromSeconds(22);
            System.Threading.Thread.Sleep(10000);

            RenderTargetBitmap rtb = new RenderTargetBitmap(320, 240, 96, 96, PixelFormats.Pbgra32);
            DrawingVisual dv = new DrawingVisual();
            DrawingContext dc = dv.RenderOpen();
            dc.DrawVideo(player, new Rect(0, 0, 320, 240));
            dc.Close();
            rtb.Render(dv);

            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            MemoryStream memoryStream = new MemoryStream();
            BitmapImage bImg = new BitmapImage();
            encoder.Frames.Add(BitmapFrame.Create(rtb));
            encoder.Save(memoryStream);
            bImg.BeginInit();
            bImg.StreamSource = new MemoryStream(memoryStream.ToArray());
            bImg.EndInit();
            memoryStream.Close();

            return bImg;
        }

        public MovieFile(String path)
        {
            var tags = TagLib.File.Create(path);
            Path = path;
            Artist = "";
            Duration = tags.Properties.Duration;
            Title = tags.Name.Substring(0,tags.Name.LastIndexOf('.')).Split("\\".ToCharArray()).Last();
            Type = ClickStyle.VIDEO;
            MessageColor = Colors.White;
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
                                   MessageColor = Colors.White };
        }
    }
}
