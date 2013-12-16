using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsMedia.classes
{
    class MusicLibrary : System.Collections.IEnumerable
    {
        public List<String> Sources { get; set; }
        public Dictionary<String, MusicArtist> Artists { get; private set; }
        public static String[] Extensions = { ".mp3", ".flac" };

        public MusicLibrary(List<String> sources)
        {
            Artists = new Dictionary<String, MusicArtist>();
            Sources = sources;
        }

        public System.Collections.IEnumerator GetEnumerator()
        {
            var artists = from pair in Artists orderby pair.Value.Name ascending select pair.Value;
            foreach (MusicArtist artist in artists)
            {
                foreach (MusicAlbum album in artist)
                {
                    yield return album;
                }
            }
        }

        public void GenerateLibrary()
        {
            Artists.Clear();
            var musics = new List<string>();
            foreach (String dir in Sources)
            {
                var files = Directory.GetFileSystemEntries(dir, "*.*", SearchOption.AllDirectories).Where(
                    s => Extensions.Contains(Path.GetExtension(s)));
                foreach (String file in files)
                {
                    if (!musics.Contains(file))
                    {
                        var tags = TagLib.File.Create(file);
                        MusicArtist artist = null;
                        if (!Artists.ContainsKey(tags.Tag.FirstPerformer))
                        {
                            artist = new MusicArtist(tags.Tag.FirstPerformer);
                            Artists.Add(artist.Name, artist);
                        }
                        else
                            artist = Artists[tags.Tag.FirstPerformer];
                        var album = artist.GetOrCreateAlbum(tags.Tag.Album);
                        album.AddTitle(tags, file);
                        musics.Add(file);
                    }
                }
            }
        }
    }
}
