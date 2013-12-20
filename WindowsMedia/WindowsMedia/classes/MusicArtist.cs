using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace WindowsMedia.classes
{
    public class MusicArtist : System.Collections.IEnumerable
    {
        public String Name { get; private set; }
        public Dictionary<String, MusicAlbum> Albums { get; private set; }

        public MusicArtist(String name)
        {
            Name = name;
            Albums = new Dictionary<String, MusicAlbum>();
        }

        public System.Collections.IEnumerator GetEnumerator()
        {
            var albums = from pair in Albums orderby pair.Value.Name ascending select pair.Value;
            foreach (MusicAlbum album in albums)
            {
                yield return album;
            }
        }

        public MusicAlbum GetOrCreateAlbum(String name)
        {
            MusicAlbum album = null;
            lock (Albums)
            {
                if (Albums.ContainsKey(name))
                    album = Albums[name];
                else
                {
                    album = new MusicAlbum(this, name);
                    Albums.Add(name, album);
                }
            }
            return album;
        }
    }
}
