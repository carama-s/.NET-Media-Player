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
    public partial class AddWindow : MetroWindow
    {
        private MainWindow ParentWindow { get; set; }

        public AddWindow(MainWindow parent)
        {
            ParentWindow = parent;
            InitializeComponent();
            AddHandler(Keyboard.KeyDownEvent, (KeyEventHandler)WindowKeyDown);
            this.Top = ParentWindow.Top + (ParentWindow.Height / 2) - (this.Height / 2);
            this.Left = ParentWindow.Left + (ParentWindow.Width / 2) - (this.Width / 2);
            if (this.ParentWindow.PlaylistBox.Items.Count <= 0)
                this.LabelWarningPlaylist.Visibility = System.Windows.Visibility.Visible;
        }
        
        private void WindowKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                ButtonAnnuler_Click(sender, null);
        }

        private void BoutonValider_Click(object sender, RoutedEventArgs e)
        {
            this.LabelWarningEmpty.Visibility = System.Windows.Visibility.Hidden;
            this.LabelWarningUsed.Visibility = System.Windows.Visibility.Hidden;
            if (this.ParentWindow.PlaylistBox.Items.Count <= 0)
                this.LabelWarningPlaylist.Visibility = System.Windows.Visibility.Visible;
            else if (TextBoxAdd.Text == "")
                this.LabelWarningEmpty.Visibility = System.Windows.Visibility.Visible;
            else if (ParentWindow.lib_.Playlists.Find(x => x.Name == TextBoxAdd.Text) != null)
                this.LabelWarningUsed.Visibility = System.Windows.Visibility.Visible;
            else
            {
                Playlist actual = new Playlist();
                foreach (var item in this.ParentWindow.PlaylistBox.Items)
                    actual.AddItem((MediaItem)((MediaItem)item).Clone());
                actual.Name = TextBoxAdd.Text;
                actual.SaveToFile();
                lock (ParentWindow.lib_.Playlists)
                {
                    ParentWindow.lib_.Playlists.Add(actual);
                    ParentWindow.MainBox.ItemsSource = ParentWindow.lib_.Playlists;
                }
                this.Close();
            }
        }

        private void ButtonAnnuler_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
