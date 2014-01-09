using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace WindowsMedia.classes
{
    public class Playlist
    {
        public List<MediaItem> Medias { get; private set; }
        public String Name { get; set; }
        public Uri Image { get; set; }

        public Playlist()
        {
            Name = "";
            Medias = new List<MediaItem>();
        }

        public Playlist(String path)
        {
            Name = Path.GetFileNameWithoutExtension(path);
            Medias = new List<MediaItem>();
            Deserialize(path);
        }

        public Playlist(List<MediaItem> medias)
        {
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
            Uri[] tab = new Uri[] { new Uri("../assets/blueplaylisticon.jpg", UriKind.Relative), new Uri("../assets/greenplaylisticon.jpg", UriKind.Relative), new Uri("../assets/pinkplaylisticon.jpg", UriKind.Relative), new Uri("../assets/purpleplaylisticon.jpg", UriKind.Relative), new Uri("../assets/redplaylisticon.jpg", UriKind.Relative) };
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
            this.Image = tab[rd.Next(5)];
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
    }
}
