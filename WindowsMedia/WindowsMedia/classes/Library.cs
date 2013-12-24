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
        public delegate void MtPtr(object pars);
        public List<MediaItem> Medias { get; private set; }
        public static String MusicPath = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
        public static String[] MusicExtensions = { ".mp3", ".flac" };
        public static String VideoPath = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
        public static String[] VideoExtensions = { ".mp4", ".mkv", ".avi", ".wmv" };
        public static String ImagePath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
        public static String[] ImageExtensions = { ".jpg", ".jpeg", ".png", ".bmp" };

        public Library()
        {
            Medias = new List<MediaItem>();
        }


        public void GenerateMusic(object param)
        {
            var pars = (Tuple<string, ManualResetEvent>)param;
            var media = new MusicTitle(pars.Item1);
            lock (Medias)
                Medias.Add(media);
            pars.Item2.Set();
        }

        public void GenerateVideo(object param)
        {
            var pars = (Tuple<string, ManualResetEvent>)param;
            var media = new MovieFile(pars.Item1);
            lock (Medias)
                Medias.Add(media);
            pars.Item2.Set();
        }

        public void GenerateImage(object param)
        {
            var pars = (Tuple<string, ManualResetEvent>)param;
            var media = new ImageFile(pars.Item1);
            lock (Medias)
                Medias.Add(media);
            pars.Item2.Set();
        }


        public void GenerateLibrary()
        {
            Medias.Clear();
            var paths = new List<string>();
            var handlers = new List<ManualResetEvent>();
            var tmps = new List<Tuple<String, String[], MtPtr>> {
                Tuple.Create(MusicPath, MusicExtensions, new MtPtr(GenerateMusic)),
                Tuple.Create(VideoPath, VideoExtensions, new MtPtr(GenerateVideo)),
                Tuple.Create(ImagePath, ImageExtensions, new MtPtr(GenerateImage))
            };
            foreach (var tmp in tmps)
            {
                var files = Directory.GetFileSystemEntries(tmp.Item1, "*.*", SearchOption.AllDirectories).Where(p => tmp.Item2.Contains(Path.GetExtension(p)));
                foreach (String file in files)
                {
                    if (!paths.Contains(file))
                    {
                        paths.Add(file);
                        var handler = new ManualResetEvent(false);
                        ThreadPool.QueueUserWorkItem(tmp.Item3.Invoke, Tuple.Create(file, handler));
                        handlers.Add(handler);
                    }
                }
            }
            foreach (var handler in handlers)
                handler.WaitOne();
        }
    }
}
