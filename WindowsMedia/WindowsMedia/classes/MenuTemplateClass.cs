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

    class TreeMenuTemplateClass
    {
        public ObservableCollection<TreeSubMenuTemplateClass> SubMenu { get; set; }

        public BitmapImage Image { get; set; }
        public string Name { get; set; }

        public TreeMenuTemplateClass(string name, string image)
        {
            this.SubMenu = new ObservableCollection<TreeSubMenuTemplateClass>();
            this.Name = name;
            string packUri = "../assets/"+ image;
            this.Image = new BitmapImage(new Uri(packUri, UriKind.Relative));
        }
    }

    class TreeSubMenuTemplateClass
    {
        public BitmapImage Image { get; set; }
        public string Name { get; set; }
        
        public TreeSubMenuTemplateClass(string name, string image)
        {
            this.Name = name;
            string packUri = "../assets/"+ image;
            this.Image = new BitmapImage(new Uri(packUri, UriKind.Relative));
        }
    }
}
