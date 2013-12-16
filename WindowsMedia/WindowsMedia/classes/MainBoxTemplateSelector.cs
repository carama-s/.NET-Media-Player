using MWMPV2.classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace WindowsMedia.classes
{
    class MainBoxTemplateSelector : DataTemplateSelector
    {
        public DataTemplate MusicTemplate { get; set; }
        public DataTemplate ArtistTemplate { get; set; }
        public DataTemplate GenreTemplate { get; set; }
        public DataTemplate VideoTemplate { get; set; }
        public DataTemplate ImageTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item != null)
            {
                if (item is MusicAlbum)
                    return MusicTemplate;
            }
            //DependencyPropertyInfo dpi = item as DependencyPropertyInfo;
            //if (dpi.PropertyType == typeof(bool))
            //{
            //    return BooleanDataTemplate;
            //}
            //if (dpi.PropertyType.IsEnum)
            //{
            //    return EnumDataTemplate;
            //}
            return base.SelectTemplate(item, container);
        }
    }
}
