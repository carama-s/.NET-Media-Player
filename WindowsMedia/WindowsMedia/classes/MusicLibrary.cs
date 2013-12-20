using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WindowsMedia.classes
{
    public class MusicLibrary : System.Collections.IEnumerable
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

        public void GenerateOneFile(object param)
        {
            var pars = (Tuple<string, ManualResetEvent>)param;
            var tags = TagLib.File.Create(pars.Item1);
            MusicArtist artist = null;
            lock (Artists)
            {
                if (!Artists.ContainsKey(tags.Tag.FirstPerformer))
                {
                    artist = new MusicArtist(tags.Tag.FirstPerformer);
                    Artists.Add(artist.Name, artist);
                }
                else
                    artist = Artists[tags.Tag.FirstPerformer];
            }
            var album = artist.GetOrCreateAlbum(tags.Tag.Album);
            album.AddTitle(tags, pars.Item1);
            pars.Item2.Set();
        }

        public void GenerateLibrary()
        {
            Artists.Clear();
            var musics = new List<string>();
            var handlers = new List<ManualResetEvent>();
            foreach (String dir in Sources)
            {
                var files = Directory.GetFileSystemEntries(dir, "*.*", SearchOption.AllDirectories).Where(s => Extensions.Contains(Path.GetExtension(s)));
                foreach (String file in files)
                {
                    if (!musics.Contains(file))
                    {
                        musics.Add(file);
                        var handler = new ManualResetEvent(false);
                        ThreadPool.QueueUserWorkItem(GenerateOneFile, Tuple.Create(file, handler));
                        handlers.Add(handler);
                    }
                }
            }
            foreach (var handler in handlers)
                handler.WaitOne();
        }
    }
}
