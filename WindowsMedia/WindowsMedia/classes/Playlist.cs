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
        [XmlElement("PlayListName")]
        public String Name { get; set; }
        [XmlArray("Elements")]
        public List<PlaylistElement> List { get; private set;}

        public Playlist()
        {
            this.List = new List<PlaylistElement>();
        }

        public void AddItem(string name, string path)
        {
            this.List.Add(new PlaylistElement(name, path));
        }

        public void RemoveItem(int index)
        {
            this.List.RemoveAt(index);
        }

        public static Playlist DeserializePlaylist(string path)
        {
            try
            {
                using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    XmlSerializer xml = new XmlSerializer(typeof(Playlist));
                    return (xml.Deserialize(fs) as Playlist);
                }
            }
            catch (IOException)
            {
                return null;
            }
        }

        public static void SerializePlaylist(Playlist list)
        {
            string direct = Environment.SpecialFolder.Personal +  "\\Playlist";
            Directory.CreateDirectory(direct);
            using(var fs = new FileStream(direct + "\\" + list.Name + ".xml",FileMode.OpenOrCreate, FileAccess.Write))
            {
                XmlSerializer xml = new XmlSerializer(typeof(Playlist));
                xml.Serialize(fs, list);
            }
        }
    }
}
