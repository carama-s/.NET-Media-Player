using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace WindowsMedia.classes
{
    public class ComboBoxWindows8 : ComboBox
    {
        public ComboBoxWindows8()
        {
            Loaded += ComboBoxWindows8_Loaded;
        }

        void ComboBoxWindows8_Loaded(object sender, RoutedEventArgs e)
        {
            ControlTemplate ct = Template;
            Border border = ct.FindName("Border", this) as Border;

            if (border != null)
            {
                border.Background = Background;

                BindingExpression be = GetBindingExpression(ComboBoxWindows8.BackgroundProperty);
                if (be != null)
                {
                    border.SetBinding(Border.BackgroundProperty, be.ParentBindingBase);
                }
            }
        }
    }
}
