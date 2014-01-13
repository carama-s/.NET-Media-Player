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
    /// 
    public class LangItem
    {
        public string Text { get; set; }
        public BitmapImage Image { get; set; }

        public LangItem(string lang, BitmapImage image)
        {
            Text = lang;
            Image = image;
        }
    }

    public partial class LangWindow : MetroWindow
    {
        private MainWindow ParentWindow { get; set; }

        public LangWindow(MainWindow parent)
        {
            List<LangItem> Display = new List<LangItem> { new LangItem(Properties.Resources.StringFrench, Properties.Resources.German), new LangItem(Properties.Resources.StringEnglish, null), new LangItem(Properties.Resources.StringDeutch, null), new LangItem(Properties.Resources.StringSpanish, null), new LangItem(Properties.Resources.StringItalian, null) };
            ParentWindow = parent;
            InitializeComponent();
            AddHandler(Keyboard.KeyDownEvent, (KeyEventHandler)WindowKeyDown);
            this.Top = ParentWindow.Top + (ParentWindow.Height / 2) - (this.Height / 2);
            this.Left = ParentWindow.Left + (ParentWindow.Width / 2) - (this.Width / 2);
            this.ListBoxLang.ItemsSource = Display;
        }

        private void WindowKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                ButtonCancel_Click(sender, null);
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            ConfigFile.Instance.Data.BiblioFiles = new List<string>();
            foreach (var item in ListBoxLang.Items)
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
