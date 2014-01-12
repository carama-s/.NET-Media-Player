using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace WindowsMedia.classes
{
    public class PlaylistSearchIterator : System.Collections.IEnumerable
    {
        public Library Library { get; private set; }
        public String Match { get; private set; }

        public PlaylistSearchIterator(Library lib, String match)
        {
            Library = lib;
            Match = match.ToLower();
        }

        public System.Collections.IEnumerator GetEnumerator()
        {
            lock (Library.Playlists)
            {
                var playlists = from playlist in Library.Playlists where playlist.Name.ToLower().Contains(Match) select playlist;
                foreach (var playlist in playlists)
                    yield return playlist;
            }
        }
    }

    public class MusicSearchIterator : System.Collections.IEnumerable
    {
        public Library Library { get; private set; }
        public String Match { get; private set; }

        public MusicSearchIterator(Library lib, String match)
        {
            Library = lib;
            Match = match.ToLower();
        }

        public System.Collections.IEnumerator GetEnumerator()
        {
            lock (Library.Medias)
            {
                var titles = from media in Library.Medias where media is MusicTitle select (MusicTitle)media;
                var musics = from title in titles
                             where title.Album.ToLower().Contains(Match) || title.Artist.ToLower().Contains(Match) || title.Title.ToLower().Contains(Match)
                             orderby title.Title
                             select title;
                foreach (var music in musics)
                    yield return music;
            }
        }
    }

    public class ImageSearchIterator : System.Collections.IEnumerable
    {
        public Library Library { get; private set; }
        public String Match { get; private set; }

        public ImageSearchIterator(Library lib, String match)
        {
            Library = lib;
            Match = match.ToLower();
        }

        public System.Collections.IEnumerator GetEnumerator()
        {
            lock (Library.Medias)
            {
                var titles = from media in Library.Medias where media is ImageFile select (ImageFile)media;
                var pics = from title in titles
                           where title.Title.ToLower().Contains(Match)
                           orderby title.Title
                           select title;
                foreach (var pic in pics)
                    yield return pic;
            }
        }
    }
    public class MovieSearchIterator : System.Collections.IEnumerable
    {
        public Library Library { get; private set; }
        public String Match { get; private set; }

        public MovieSearchIterator(Library lib, String match)
        {
            Library = lib;
            Match = match.ToLower();
        }

        public System.Collections.IEnumerator GetEnumerator()
        {
            lock (Library.Medias)
            {

                var titles = from media in Library.Medias where media is MovieFile select (MovieFile)media;
                var musics = from title in titles
                             where title.Title.ToLower().Contains(Match)
                             orderby title.Title
                             select title;
                foreach (var music in musics)
                    yield return music;
            }
        }
    }

    public class AlbumIterator : System.Collections.IEnumerable
    {
        public Library Library { get; private set; }

        public AlbumIterator(Library lib)
        {
            Library = lib;
        }

        public System.Collections.IEnumerator GetEnumerator()
        {
            lock (Library.Medias)
            {
                var titles = from media in Library.Medias where media is MusicTitle select (MusicTitle)media;
                var albums = from title in titles
                             orderby title.Album, title.TrackNumber, title.Title //orderby title.Album, title.Artist, title.TrackNumber, title.Title
                             group title by title.Album; //group title by new { title.Artist, title.Album };
                foreach (var album in albums)
                    yield return album.ToList();
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
            lock (Library.Medias)
            {
                var titles = from media in Library.Medias where media is MusicTitle select (MusicTitle)media;
                var artists = from title in titles
                              orderby title.Artist, title.Album, title.TrackNumber, title.Title
                              group title by title.Artist;
                foreach (var artist in artists)
                    yield return new MusicArtist(artist.Key, artist.ToList());
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
            lock (Library.Medias)
            {

                var titles = from media in Library.Medias where media is MusicTitle select (MusicTitle)media;
                var genres = from title in titles
                             orderby title.Genre, title.Album, title.TrackNumber, title.Title
                             group title by title.Genre;
                foreach (var genre in genres)
                    yield return new MusicArtist(genre.Key, genre.ToList());
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
            lock (Library.Medias)
            {
                var images = from media in Library.Medias where media is ImageFile select (ImageFile)media;
                var sorted = from image in images orderby Path.GetFileName(image.Path) select image;
                foreach (var image in sorted)
                {
                    yield return image;
                }
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
            lock (Library.Medias)
            {
                var movies = from media in Library.Medias where media is MovieFile select (MovieFile)media;
                var sorted = from movie in movies orderby Path.GetFileName(movie.Path) select movie;
                foreach (var movie in sorted)
                    yield return movie;
            }
        }
    }

    public class Library
    {
        public delegate void MtPtr(object pars);
        public List<MediaItem> Medias { get; private set; }
        public List<Playlist> Playlists { get; private set; }
        public List<String> BiblioPath { get; set; }
        private Boolean GenerateMutex = false;
        private object GenerateMutexLock = new object();
        private MainWindow MW { get; set; }
        private List<String> Paths { get; set; }

        public static String MusicPath = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
        public static String VideoPath = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
        public static String ImagePath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
        public static String PlaylistPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Playlists");
        public static String[] PlaylistExtensions = { ".m3u" };

        public Library(MainWindow mw)
        {
            MW = mw;
            Paths = new List<string>();
            Medias = new List<MediaItem>();
            Playlists = new List<Playlist>();
        }

        public void GenerateMedia(object path)
        {
            var media = MediaItem.Create((string)path);
            if (media != null)
                lock (Medias)
                    Medias.Add(media);
        }

        public void GeneratePlaylist(object path)
        {
            var playlist = new Playlist((string) path);
            lock (Playlists)
                Playlists.Add(playlist);
        }

        public void GenerateLibraryThreadBis(object param)
        {
            var tmp = (Tuple<String, String[], MtPtr, ManualResetEvent, ClickStyle, int>)param;
            int counter = 0;
            try
            {
                foreach (var ext in tmp.Item2)
                {
                    var files = Directory.GetFiles(tmp.Item1, "*" + ext, SearchOption.AllDirectories);
                    foreach (String file in files)
                    {
                        var cont = false;
                        lock (Paths)
                        {
                            cont = Paths.Contains(file);
                            if (!cont)
                                Paths.Add(file);
                        }
                        if (!cont)
                        {
                            tmp.Item3.Invoke(file);
                            if (++counter >= tmp.Item6)
                            {
                                lock (MW)
                                {
                                    MW.UpdateCurrentPanel(tmp.Item5);
                                }
                                counter = 0;
                            }
                        }
                    }
                }
                if (counter > 0)
                {
                    lock (MW)
                    {
                        MW.UpdateCurrentPanel(tmp.Item5);
                    }
                }
            }
            catch (DirectoryNotFoundException)
            {
                try
                {
                    Directory.CreateDirectory(tmp.Item1);
                }
                catch (Exception) { }
            }
            catch (ArgumentException)
            {
                ConfigFile.Instance.Data.BiblioFiles.Remove(tmp.Item1);
            }
            catch (Exception) { }
            tmp.Item4.Set();
        }

        public void GenerateLibraryThread(object param)
        {
            lock (GenerateMutexLock)
            {
                if (GenerateMutex)
                    return;
                GenerateMutex = true;
            }
            lock (Medias)
            {
                Medias.Clear();
            }
            lock (Playlists)
            {
                Playlists.Clear();
            }
            Paths.Clear();
            var ptr = new MtPtr(GenerateMedia);
            var tmps = new List<Tuple<String, String[], MtPtr, ManualResetEvent, ClickStyle, int>> {
                Tuple.Create(MusicPath, MediaItem.MusicExtensions, ptr, new ManualResetEvent(false), ClickStyle.MUSIC, 100),
                Tuple.Create(VideoPath, MediaItem.VideoExtensions, ptr, new ManualResetEvent(false), ClickStyle.VIDEO, 1),
                Tuple.Create(ImagePath, MediaItem.ImageExtensions, ptr, new ManualResetEvent(false), ClickStyle.IMAGE, 1),
                Tuple.Create(PlaylistPath, PlaylistExtensions, new MtPtr(GeneratePlaylist), new ManualResetEvent(false), ClickStyle.SELECTION, 1)
            };
            foreach (var dir in ConfigFile.Instance.Data.BiblioFiles)
            {
                tmps.Add(Tuple.Create(dir, MediaItem.MusicExtensions, ptr, new ManualResetEvent(false), ClickStyle.MUSIC, 100));
                tmps.Add(Tuple.Create(dir, MediaItem.VideoExtensions, ptr, new ManualResetEvent(false), ClickStyle.VIDEO, 1));
                tmps.Add(Tuple.Create(dir, MediaItem.ImageExtensions, ptr, new ManualResetEvent(false), ClickStyle.IMAGE, 1));
            }
            foreach (var tmp in tmps)
                ThreadPool.QueueUserWorkItem(GenerateLibraryThreadBis, tmp);
            foreach (var tmp in tmps)
                tmp.Item4.WaitOne();
            lock (GenerateMutexLock)
            {
                GenerateMutex = false;
            }
        }

        public void GenerateLibrary()
        {
            ThreadPool.QueueUserWorkItem(GenerateLibraryThread);
        }
    }
}
