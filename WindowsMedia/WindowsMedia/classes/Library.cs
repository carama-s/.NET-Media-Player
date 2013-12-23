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
        public Library Library { get; private set; }

        public AlbumIterator(Library lib)
        {
            Library = lib;
        }

        public System.Collections.IEnumerator GetEnumerator()
        {
            var titles = from media in Library.Medias where media is MusicTitle select (MusicTitle)media;
            var albums = from title in titles
                         orderby title.Album, title.Artist, title.TrackNumber, title.Title
                         group title by new { title.Artist, title.Album };
            foreach (var album in albums)
            {
                yield return new List<MusicTitle>(album);
            }
        }
    }

    public class ArtistIterator : System.Collections.IEnumerable
    {
        public Library Library { get; private set; }

        public ArtistIterator(Library lib)
        {
            Library = lib;
        }

        public System.Collections.IEnumerator GetEnumerator()
        {
            var titles = from media in Library.Medias where media is MusicTitle select (MusicTitle)media;
            var albums = from title in titles
                         orderby title.Artist, title.Album, title.TrackNumber, title.Title
                         group title by new { title.Artist, title.Album };
            foreach (var album in albums)
            {
                yield return new List<MusicTitle>(album);
            }
        }
    }

    public class GenreIterator : System.Collections.IEnumerable
    {
        public Library Library { get; private set; }

        public GenreIterator(Library lib)
        {
            Library = lib;
        }

        public System.Collections.IEnumerator GetEnumerator()
        {
            var titles = from media in Library.Medias where media is MusicTitle select (MusicTitle)media;
            var albums = from title in titles
                         orderby title.Genre, title.Album, title.Artist, title.TrackNumber, title.Title
                         group title by new { title.Artist, title.Album };
            foreach (var album in albums)
            {
                yield return new List<MusicTitle>(album);
            }
        }
    }

    public class ImageIterator : System.Collections.IEnumerable
    {
        public Library Library { get; private set; }

        public ImageIterator(Library lib)
        {
            Library = lib;
        }

        public System.Collections.IEnumerator GetEnumerator()
        {
            var images = from media in Library.Medias where media is ImageFile select (ImageFile)media;
            var sorted = from image in images orderby Path.GetFileName(image.Path) select image;
            foreach (var image in sorted)
            {
                yield return image;
            }
        }
    }

    public class MovieIterator : System.Collections.IEnumerable
    {
        public Library Library { get; private set; }

        public MovieIterator(Library lib)
        {
            Library = lib;
        }

        public System.Collections.IEnumerator GetEnumerator()
        {
            var movies = from media in Library.Medias where media is MovieFile select (MovieFile)media;
            var sorted = from movie in movies orderby Path.GetFileName(movie.Path) select movie;
            foreach (var movie in sorted)
            {
                yield return movie;
            }
        }
    }

    public class Library
    {
        public List<String> Sources { get; set; }
        public List<MediaItem> Medias { get; private set; }
        public static String[] MusicExtensions = { ".mp3", ".flac" };
        public static String[] VideoExtensions = { ".mp4", ".mkv", ".avi", ".wmv" };
        public static String[] ImageExtensions = { ".jpg", ".jpeg", ".png", ".bmp" };

        public Library(List<String> sources)
        {
            Medias = new List<MediaItem>();
            Sources = sources;
        }

        public void GenerateMusic(String path)
        {
            var title = new MusicTitle(path);
        }

        public void GenerateFile(object param)
        {
            var pars = (Tuple<string, ManualResetEvent>)param;
            var extension = Path.GetExtension(pars.Item1);
            MediaItem media = null;
            if (MusicExtensions.Contains(extension))
                media = new MusicTitle(pars.Item1);
            else if (VideoExtensions.Contains(extension))
                media = new MovieFile(pars.Item1);
            else
                media = new ImageFile(pars.Item1);
            lock (Medias)
                Medias.Add(media);
            pars.Item2.Set();
        }

        public Boolean GoodExtension(String path)
        {
            var extension = Path.GetExtension(path);
            return (MusicExtensions.Contains(extension) || VideoExtensions.Contains(extension) || ImageExtensions.Contains(extension));
        }

        public void GenerateLibrary()
        {
            Medias.Clear();
            var paths = new List<string>();
            var handlers = new List<ManualResetEvent>();
            foreach (String dir in Sources)
            {
                var files = Directory.GetFileSystemEntries(dir, "*.*", SearchOption.AllDirectories).Where(GoodExtension);
                foreach (String file in files)
                {
                    if (!paths.Contains(file))
                    {
                        paths.Add(file);
                        var handler = new ManualResetEvent(false);
                        ThreadPool.QueueUserWorkItem(GenerateFile, Tuple.Create(file, handler));
                        handlers.Add(handler);
                    }
                }
            }
            foreach (var handler in handlers)
                handler.WaitOne();
        }
    }
}
