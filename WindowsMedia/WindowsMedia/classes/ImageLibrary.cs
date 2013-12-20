using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsMedia.classes
{
    public class ImageLibrary : System.Collections.IEnumerable
    {
        public List<string> Sources { get; private set; }
        public List<ImageFile> Images { get; private set; }
        public static String[] Extensions = { ".jpg", ".jpeg", ".png", ".bmp" };

        public ImageLibrary(List<String> sources)
        {
            Sources = sources;
            Images = new List<ImageFile>();
        }

        public System.Collections.IEnumerator GetEnumerator()
        {
            var images = from image in Images orderby Path.GetFileName(image.Path) ascending select image;
            foreach (ImageFile image in images)
            {
                yield return image;
            }
        }

        public void GenerateLibrary()
        {
            Images.Clear();
            var paths = new List<String>();
            foreach (String dir in Sources)
            {
                var files = Directory.GetFileSystemEntries(dir, "*.*", SearchOption.AllDirectories).Where(
                    s => Extensions.Contains(Path.GetExtension(s)));
                foreach (String file in files)
                {
                    if (!paths.Contains(file))
                    {
                        Images.Add(new ImageFile(file));
                    }
                }
            }
        }
    }
}
