using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsMedia.classes;

namespace WindowsMedia
{
    class ConfigFile
    {
        public int Height { get { return 600; } set { } }
        public int Width { get { return 800; } set { } }
        public bool Muted { get { return false; } set { } }
        public bool Shuffle { get { return false; } set { } }
        public bool Repeat { get { return false; } set { } }
        public int Volume { get { return 50; } set { } }
        public MusicStyle Style { get { return MusicStyle.ALBUM; } set { } }
        public List<String> BilioFiles { get { return new List<String> { Library.MusicPath, Library.ImagePath, Library.VideoPath, Library.PlaylistPath }; } set { } }
    }
}
