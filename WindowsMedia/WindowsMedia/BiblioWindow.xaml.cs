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
            List<String> Display;
            ParentWindow = parent;
            InitializeComponent();
            AddHandler(Keyboard.KeyDownEvent, (KeyEventHandler)WindowKeyDown);
            this.Title = "Gérer la bibliothèque";
            this.Top = ParentWindow.Top + (ParentWindow.Height / 2) - (this.Height / 2);
            this.Left = ParentWindow.Left + (ParentWindow.Width / 2) - (this.Width / 2);
            Display = new List<string>(ConfigFile.Instance.Data.BiblioFiles);
            foreach (string path in Display)
            {
                ListBoxBiblio.Items.Add(path);
            }
        }

        private void WindowKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                ButtonCancel_Click(sender, null);
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog();
            dlg.ShowDialog();
            if (dlg.SelectedPath != "" && !ListBoxBiblio.Items.Contains(dlg.SelectedPath))
                ListBoxBiblio.Items.Add(dlg.SelectedPath);
        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            if (ListBoxBiblio.SelectedItem != null)
            {
                ListBoxBiblio.Items.RemoveAt(ListBoxBiblio.SelectedIndex);
            }
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            ConfigFile.Instance.Data.BiblioFiles = new List<string>();
            foreach (var item in ListBoxBiblio.Items)
            {
                ConfigFile.Instance.Data.BiblioFiles.Add((String)item);
            }
            this.Close();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
