using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;

namespace WindowsMedia.classes
{
    public class Playlist
    {
        public List<MediaItem> Medias { get; private set; }
        public String Name { get; set; }
        public BitmapImage Image { get; set; }
        private static object Lock = new object();
        private static int ColorChooser = 0;

        public Playlist()
        {
            SetImage();
            Name = "";
            Medias = new List<MediaItem>();
        }

        public Playlist(String path)
        {
            SetImage();
            Name = Path.GetFileNameWithoutExtension(path);
            Medias = new List<MediaItem>();
            Deserialize(path);
        }

        public Playlist(List<MediaItem> medias)
        {
            SetImage();
            Name = "";
            Medias = new List<MediaItem>(medias);
        }

        public void AddItem(MediaItem media)
        {
            Medias.Add(media);
        }

        public void RemoveItem(int index)
        {
            Medias.RemoveAt(index);
        }

        public void RemoveItem(MediaItem media)
        {
            Medias.Remove(media);
        }

        public void SaveToFile()
        {
            var file = new StreamWriter(Path.Combine(Library.PlaylistPath, Name + ".m3u"));
            file.Write(Serialize());
            file.Close();
        }

        public void Deserialize(String path)
        {
            var file = new StreamReader(path);
            String line;
            int index = 0;
            Random rd = new Random();
            while ((line = file.ReadLine()) != null)
            {
                if (line.Length > 0 && line[0] != '#')
                {
                    var media = MediaItem.Create(line);

                    if (media != null)
                    {
                        media.SetIndex(index);
                        index++;
                        Medias.Add(media);
                    }
                }
            }
            file.Close();
        }

        public String Serialize()
        {
            var output = "";

            foreach (var media in Medias)
            {
                output += media.Path + '\n';
            }
            return output;
        }

        public void SetImage()
        {
            lock (Lock)
            {
                Image = DefaultImageGetter.Instance.Playlists[ColorChooser++];
                if (ColorChooser >= DefaultImageGetter.Instance.Playlists.Count)
                    ColorChooser = 0;
            }
        }
    }
}
