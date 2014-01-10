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
    /// Interaction logic for RenameWindow.xaml
    /// </summary>
    public partial class RenameWindow : MetroWindow
    {
        private MainWindow ParentWindow { get; set; }
        private string InitName { get; set; }

        public RenameWindow(MainWindow parent)
        {
            ParentWindow = parent;
            InitializeComponent();
            this.Title = "Renommer une sélection";
            AddHandler(Keyboard.KeyDownEvent, (KeyEventHandler)WindowKeyDown);
            this.Top = ParentWindow.Top + (ParentWindow.Height / 2) - (this.Height / 2);
            this.Left = ParentWindow.Left + (ParentWindow.Width / 2) - (this.Width / 2);
            TextBoxRename.Text = ((Playlist)ParentWindow.MainBox.SelectedItem).Name;
            InitName = TextBoxRename.Text;
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
            if (TextBoxRename.Text == "")
                this.LabelWarningEmpty.Visibility = System.Windows.Visibility.Visible;
            else if (ParentWindow.lib_.Playlists.Find(x => x.Name == TextBoxRename.Text) != null)
                this.LabelWarningUsed.Visibility = System.Windows.Visibility.Visible;
            else
            {
                System.IO.File.Move(System.IO.Path.Combine(Library.PlaylistPath, InitName + ".m3u"), System.IO.Path.Combine(Library.PlaylistPath, TextBoxRename.Text + ".m3u"));
                ParentWindow.RefreshLib(null, null);
                this.Close();
            }
        }

        private void ButtonAnnuler_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
