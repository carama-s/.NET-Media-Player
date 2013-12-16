using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsMedia.classes
{
    public class MovieLibrary : System.Collections.IEnumerable
    {
        public List<string> Sources { get; private set; }
        public List<MovieFile> Movies { get; private set; }
        public static String[] Extensions = { ".mp4", ".mkv", ".avi", ".wmv" };

        public MovieLibrary(List<String> sources)
        {
            Sources = sources;
            Movies = new List<MovieFile>();
        }

        public System.Collections.IEnumerator GetEnumerator()
        {
            var movies = from movie in Movies orderby Path.GetFileName(movie.Path) ascending select movie;
            foreach (MovieFile movie in movies)
            {
                yield return movie;
            }
        }

        public void GenerateLibrary()
        {
            Movies.Clear();
            var paths = new List<String>();
            foreach (String dir in Sources)
            {
                var files = Directory.GetFileSystemEntries(dir, "*.*", SearchOption.AllDirectories).Where(
                    s => Extensions.Contains(Path.GetExtension(s)));
                foreach (String file in files)
                {
                    if (!paths.Contains(file))
                    {
                        Movies.Add(new MovieFile(file));
                    }
                }
            }
        }
    }
}
