using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace WindowsMedia.classes
{
    public class MusicAlbum : System.Collections.IEnumerable
    {
        public MusicArtist Artist { get; private set; }
        public String Name { get; private set; }
        public List<MusicTitle> Titles { get; private set; }

        public MusicAlbum(MusicArtist artist, String name)
        {
            Titles = new List<MusicTitle>();
            Artist = artist;
            Name = name;
        }

        public System.Collections.IEnumerator GetEnumerator()
        {
            var titles = from title in Titles orderby title.TrackNumber ascending select title;
            foreach (MusicTitle title in titles)
            {
                yield return title;
            }
        }

        public MusicTitle AddTitle(TagLib.File tags, String file)
        {
            MusicTitle title = new MusicTitle(Artist, this, tags, file);
            lock (Titles)
            {
                Titles.Add(title);
                Titles.Sort((title1, title2) => title1.TrackNumber.CompareTo(title2.TrackNumber));
            }
            return title;
        }
    }
}
