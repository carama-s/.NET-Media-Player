using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace WindowsMedia.classes
{
    [Serializable]
    public class Playlist
    {
        public List<MediaItem> Medias { get; private set; }

        public Playlist()
        {
            Medias = new List<MediaItem>();
        }

        public Playlist(String path)
        {
            Medias = new List<MediaItem>();
            Deserialize(path);
        }

        public Playlist(List<MediaItem> medias)
        {
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

        public void Deserialize(String path)
        {
            StreamReader file = new StreamReader(path);
            String line;

            while ((line = file.ReadLine()) != null)
            {
                if (line.Length > 0 && line[0] != '#')
                {
                    if (Library.MusicExtensions.Contains(Path.GetExtension(line)))
                    {
                        Medias.Add(new MusicTitle(line));
                    }
                    else if (Library.VideoExtensions.Contains(Path.GetExtension(line)))
                    {
                        Medias.Add(new MovieFile(line));
                    }
                    else if (Library.ImageExtensions.Contains(Path.GetExtension(line)))
                    {
                        Medias.Add(new ImageFile(line));
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
    }
}
