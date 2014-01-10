using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WindowsMedia
{
    /// <summary>
    /// Interaction logic for BiblioWindow.xaml
    /// </summary>
    public partial class BiblioWindow : MetroWindow
    {
        private MainWindow ParentWindow { get; set; }

        public BiblioWindow(MainWindow parent)
        {
            ParentWindow = parent;
            InitializeComponent();
            this.Title = "Gérer la bibliothèque";
            this.Top = ParentWindow.Top + (ParentWindow.Height / 2) - (this.Height / 2);
            this.Left = ParentWindow.Left + (ParentWindow.Width / 2) - (this.Width / 2);
        }
    }
}
