using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WindowsMedia.classes
{
    public class AlbumIterator : System.Collections.IEnumerable
    {
        public MusicLibrary Library { get; private set; }

        public AlbumIterator(MusicLibrary lib)
        {
            Library = lib;
        }

        public System.Collections.IEnumerator GetEnumerator()
        {
            var albums = from title in Library.Titles
                         orderby title.Album, title.Artist, title.TrackNumber
                         group title by new { title.Artist, title.Album };
            foreach (var album in albums)
            {
                yield return new List<MusicTitle>(album);
            }
        }
    }

    public class ArtistIterator : System.Collections.IEnumerable
    {
        public MusicLibrary Library { get; private set; }

        public ArtistIterator(MusicLibrary lib)
        {
            Library = lib;
        }

        public System.Collections.IEnumerator GetEnumerator()
        {
            var albums = from title in Library.Titles
                         orderby title.Artist, title.Album, title.TrackNumber
                         group title by new { title.Artist, title.Album };
            foreach (var album in albums)
            {
                yield return new List<MusicTitle>(album);
            }
        }
    }

    public class GenreIterator : System.Collections.IEnumerable
    {
        public MusicLibrary Library { get; private set; }

        public GenreIterator(MusicLibrary lib)
        {
            Library = lib;
        }

        public System.Collections.IEnumerator GetEnumerator()
        {
            var albums = from title in Library.Titles
                         orderby title.Genre, title.Album, title.Artist, title.TrackNumber
                         group title by new { title.Artist, title.Album };
            foreach (var album in albums)
            {
                yield return new List<MusicTitle>(album);
            }
        }
    }

    public class MusicLibrary
    {
        public List<String> Sources { get; set; }
        public List<MusicTitle> Titles { get; private set; }
        public static String[] Extensions = { ".mp3", ".flac" };

        public MusicLibrary(List<String> sources)
        {
            Titles = new List<MusicTitle>();
            Sources = sources;
        }

        public void GenerateOneFile(object param)
        {
            var pars = (Tuple<string, ManualResetEvent>)param;
            var title = new MusicTitle(pars.Item1);
            lock (Titles)
                Titles.Add(title);
            pars.Item2.Set();
        }

        public void GenerateLibrary()
        {
            Titles.Clear();
            var paths = new List<string>();
            var handlers = new List<ManualResetEvent>();
            foreach (String dir in Sources)
            {
                var files = Directory.GetFileSystemEntries(dir, "*.*", SearchOption.AllDirectories).Where(s => Extensions.Contains(Path.GetExtension(s)));
                foreach (String file in files)
                {
                    if (!paths.Contains(file))
                    {
                        paths.Add(file);
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
