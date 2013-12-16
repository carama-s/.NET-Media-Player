using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace WindowsMedia.classes
{
    public class MainBoxTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            FrameworkElement elem = container as FrameworkElement;

            if (elem != null && item != null)
            {
                if (item is MusicAlbum)
                    return elem.FindResource("MainMusicTemplate") as DataTemplate;
            }
            return base.SelectTemplate(item, container);
        }
    }
}
