﻿using AsfMojo.Media;
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
            /*MediaDet md = new MediaDet();
            md.Filename = Path;
            md.CurrentStream = 0;
            //string fBitmapName = "C:\\Users\\Robert\\image.jpg";
            //md.WriteBitmapBits(125.2, 320, 240, fBitmapName);
            
            //md.GetBitmapBits(125.2, bufferSize, (byte)IntPtr.Zero, 320, 200);
            buffer = (byte*)Marshal.AllocHGlobal(125000);
            md.GetBitmapBits(125.2, 125000, *buffer, 320, 200);
            var img = new BitmapImage();
            img.BeginInit();
            img.StreamSource = new MemoryStream(*buffer);
            img.EndInit();
            return img;*/
            MediaPlayer player = new MediaPlayer();
            player.Open(new Uri(Path, UriKind.RelativeOrAbsolute));
            player.Position = TimeSpan.FromSeconds(22);

            RenderTargetBitmap rtb = new RenderTargetBitmap(320, 240, 96, 96, PixelFormats.Pbgra32);
            DrawingVisual dv = new DrawingVisual();
            DrawingContext dc = dv.RenderOpen();
            dc.DrawVideo((MediaPlayer)player, new Rect(0, 0, 320, 240));
            dc.Close();
            rtb.Render(dv);

            BitmapFrame bmp = BitmapFrame.Create(rtb);
            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            MemoryStream memoryStream = new MemoryStream();
            BitmapImage bImg = new BitmapImage();
            encoder.Frames.Add(BitmapFrame.Create(bmp));
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
