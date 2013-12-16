using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WindowsMedia.classes
{
    public class MovieFile
    {
        public String Path { get; private set; }
        public TimeSpan Duration { get; private set; }

        public MovieFile(String path)
        {
            Path = path;
            var tags = TagLib.File.Create(path);
            Duration = tags.Properties.Duration;
        }
    }
}
