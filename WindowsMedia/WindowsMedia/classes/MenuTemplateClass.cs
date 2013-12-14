using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MWMPV2.classes
{
    class MenuTemplateClass
    {
        public string Name { get; set; }

        public BitmapImage Image { get; set; }

        public MenuTemplateClass(string name, string image)
        {
            this.Name = name;
            string packUri = "../assets/"+ image;
            this.Image = new BitmapImage(new Uri(packUri, UriKind.Relative));
        }
    }
}
