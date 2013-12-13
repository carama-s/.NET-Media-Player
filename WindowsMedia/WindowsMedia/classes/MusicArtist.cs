using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MWMPV2.classes
{
    class MusicArtist : System.Collections.IEnumerable
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
            if (Albums.ContainsKey(name))
                return Albums[name];
            MusicAlbum album = new MusicAlbum(this, name);
            Albums.Add(name, album);
            return album;
        }
    }
}
