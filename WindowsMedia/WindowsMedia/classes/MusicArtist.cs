using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace WindowsMedia.classes
{
    class MusicArtist : System.Collections.IEnumerable
    {
        public String Artist { get; private set; }
        public List<MusicTitle> Titles { get; private set; }
        public String Genre
        {
            get
            {
                return Artist;
            }
            private set
            {
                Artist = value;
            }
        }

        public BitmapImage Image
        {
            get
            {
                if (Titles.Count > 0)
                    return Titles[0].Image;
                else
                    return new BitmapImage(MusicTitle.DefaultImagePath);
            }
            private set;
        }

        public MusicArtist(String name, List<MusicTitle> titles)
        {
            Artist = name;
            Titles = titles;
        }

        public System.Collections.IEnumerator GetEnumerator()
        {
            var albums = from title in Titles
                         group title by title.Album;
            foreach (var album in albums)
                yield return albums.ToList();
        }
    }
}
