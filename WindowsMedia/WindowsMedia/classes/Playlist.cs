using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsMedia.classes
{
    [Serializable]
    public class Playlist
    {
        public String Name { get; set; }
        public List<PlaylistElement> List { get; private set;}

        public Playlist()
        {
            this.List = new List<PlaylistElement>();
        }

        public void addItem(string name, string path)
        {
            this.List.Add(new PlaylistElement(name, path));
        }
    }
}
