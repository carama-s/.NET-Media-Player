using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

namespace WindowsMedia.classes
{
    class WrapBoxContainerSelector : StyleSelector
    {
        public override Style SelectStyle(object item, DependencyObject container)
        {
            FrameworkElement elem = container as FrameworkElement;
            if (elem != null && item != null)
            {
                if (item is MovieFile)
                    return (elem.FindResource("WrapBoxVideoContainer") as Style);
               else if (item is ImageFile)
                    return (elem.FindResource("WrapBoxImageContainer") as Style);
            }
            return base.SelectStyle(item, container);
        }
    }
}
