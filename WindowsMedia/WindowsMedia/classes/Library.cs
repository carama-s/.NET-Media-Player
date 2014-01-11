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
            var titles = from media in Library.Medias where media is MusicTitle select (MusicTitle)media;
            var musics = from title in titles
                         where title.Album.ToLower().Contains(Match) || title.Artist.ToLower().Contains(Match) || title.Title.ToLower().Contains(Match)
                         orderby title.Title
                         select title;
            foreach (var music in musics)
            {
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
            var titles = from media in Library.Medias where media is ImageFile select (ImageFile)media;
            var pics = from title in titles
                         where title.Title.ToLower().Contains(Match)
                         orderby title.Title
                         select title;
            foreach (var pic in pics)
            {
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
            var titles = from media in Library.Medias where media is MovieFile select (MovieFile)media;
            var musics = from title in titles
                         where title.Title.ToLower().Contains(Match)
                         orderby title.Title
                         select title;
            foreach (var music in musics)
            {
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
            var titles = from media in Library.Medias where media is MusicTitle select (MusicTitle)media;
            var albums = from title in titles
                         orderby title.Album, title.Artist, title.TrackNumber, title.Title
                         group title by new { title.Artist, title.Album };
            foreach (var album in albums)
            {
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
            var titles = from media in Library.Medias where media is MusicTitle select (MusicTitle)media;
            var artists = from title in titles
                          orderby title.Artist, title.Album, title.TrackNumber, title.Title
                          group title by title.Artist;
            foreach (var artist in artists)
                yield return new MusicArtist(artist.Key, artist.ToList());
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
            var genres = from title in titles
                         orderby title.Genre, title.Album, title.Artist, title.TrackNumber, title.Title
                         group title by title.Genre;
            foreach (var genre in genres)
                yield return new MusicArtist(genre.Key, genre.ToList());
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
        private Mutex GenerateMutex { get; set; }
        public List<Playlist> Playlists { get; private set; }
        public static String MusicPath = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
        public static String VideoPath = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
        public static String ImagePath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
        public static String PlaylistPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Playlists");
        public static String[] PlaylistExtensions = { ".m3u" };
        public List<String> BiblioPath { get; set; }

        public Library()
        {
            GenerateMutex = new Mutex(false);
            Medias = new List<MediaItem>();
            Playlists = new List<Playlist>();
        }


        public void GenerateMedia(object param)
        {
            var pars = (Tuple<string, ManualResetEvent>)param;
            var media = MediaItem.Create(pars.Item1);
            if (media != null)
                lock (Medias)
                    Medias.Add(media);
            pars.Item2.Set();
        }

        public void GeneratePlaylist(object param)
        {
            var pars = (Tuple<string, ManualResetEvent>)param;
            var playlist = new Playlist(pars.Item1);
            lock (Playlists)
                Playlists.Add(playlist);
            pars.Item2.Set();
        }


        public void GenerateLibrary()
        {
            GenerateMutex.WaitOne();
            Medias.Clear();
            Playlists.Clear();
            var paths = new List<string>();
            var handlers = new List<ManualResetEvent>();
            var ptr = new MtPtr(GenerateMedia);
            var tmps = new List<Tuple<String, String[], MtPtr>> {
                Tuple.Create(MusicPath, MediaItem.MusicExtensions, ptr),
                Tuple.Create(VideoPath, MediaItem.VideoExtensions, ptr),
                Tuple.Create(ImagePath, MediaItem.ImageExtensions, ptr),
                Tuple.Create(PlaylistPath, PlaylistExtensions, new MtPtr(GeneratePlaylist))
            };
            var allExtensions = new List<String>(MediaItem.MusicExtensions);
            allExtensions.AddRange(MediaItem.VideoExtensions);
            allExtensions.AddRange(MediaItem.ImageExtensions);
            foreach (var dir in ConfigFile.Instance.Data.BiblioFiles)
                tmps.Add(Tuple.Create(dir, allExtensions.ToArray(), ptr));
            foreach (var tmp in tmps)
            {
                try
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
                catch (DirectoryNotFoundException)
                {
                    try
                    {
                        Directory.CreateDirectory(tmp.Item1);
                    }
                    catch (DirectoryNotFoundException) { }
                }
                catch (ArgumentException)
                {
                    ConfigFile.Instance.Data.BiblioFiles.Remove(tmp.Item1);
                }
            }
            foreach (var handler in handlers)
                handler.WaitOne();
            GenerateMutex.ReleaseMutex();
            int i = 0;
            foreach (var list in Playlists)
            {
                list.SetImage(i);
                i++;
            }
        }
    }
}
