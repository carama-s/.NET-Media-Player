using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsMedia.classes
{
    [Serializable]
    public class PlaylistElement
    {
        public String Name { get; private set; }
        public String Path { get; private set; }

        public PlaylistElement(String name, String path)
        {
            this.Name = name;
            this.Path = path;
        }
    }
}
