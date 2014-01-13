using MahApps.Metro.Controls;
using System;
using System.IO;
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
using WindowsMedia.classes;

namespace WindowsMedia
{
    /// <summary>
    /// Interaction logic for DeleteWindow.xaml
    /// </summary>
    public partial class DeleteWindow : MetroWindow
    {
        private MainWindow ParentWindow { get; set; }
        private string InitName { get; set; }

        public DeleteWindow(MainWindow parent)
        {
            ParentWindow = parent;
            InitializeComponent();
            AddHandler(Keyboard.KeyDownEvent, (KeyEventHandler)WindowKeyDown);
            this.Top = ParentWindow.Top + (ParentWindow.Height / 2) - (this.Height / 2);
            this.Left = ParentWindow.Left + (ParentWindow.Width / 2) - (this.Width / 2);
            InitName = ((Playlist)ParentWindow.MainBox.SelectedItem).Name;
            LabelName.Text += " \"" + InitName + "\" ?";            
        }

        private void WindowKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                ButtonAnnuler_Click(sender, null);
        }

        private void BoutonValider_Click(object sender, RoutedEventArgs e)
        {
            System.IO.File.Delete(System.IO.Path.Combine(Library.PlaylistPath, InitName + ".m3u"));
            lock (ParentWindow.lib_.Playlists)
            {
                ParentWindow.lib_.Playlists.RemoveAt(ParentWindow.lib_.Playlists.FindIndex(x => x.Name == InitName));
                ParentWindow.MainBox.ItemsSource = ParentWindow.lib_.Playlists;
            }
            this.Close();
        }

        private void ButtonAnnuler_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
