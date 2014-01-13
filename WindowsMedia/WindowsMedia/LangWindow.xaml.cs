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
        private int lang { get; set; }

        public LangWindow(MainWindow parent)
        {
            string[] lang = { Properties.Resources.StringFrench, Properties.Resources.StringEnglish, Properties.Resources.StringDeutch, Properties.Resources.StringSpanish, Properties.Resources.StringItalian };
            List<LangItem> Display = new List<LangItem> { new LangItem(lang[0], DefaultImageGetter.Instance.French), new LangItem(lang[1], DefaultImageGetter.Instance.English), new LangItem(lang[2], DefaultImageGetter.Instance.German), new LangItem(lang[3], DefaultImageGetter.Instance.Spanish), new LangItem(lang[4], DefaultImageGetter.Instance.Italian) };
            ParentWindow = parent;
            InitializeComponent();
            AddHandler(Keyboard.KeyDownEvent, (KeyEventHandler)WindowKeyDown);
            this.Top = ParentWindow.Top + (ParentWindow.Height / 2) - (this.Height / 2);
            this.Left = ParentWindow.Left + (ParentWindow.Width / 2) - (this.Width / 2);
            this.ListBoxLang.ItemsSource = Display;
            LabelLang.Content = lang[(int)parent.Lang];
        }

        private void WindowKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                ButtonCancel_Click(sender, null);
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            if (ListBoxLang.SelectedIndex != -1)
                ParentWindow.Lang = (Language)lang;
            this.Close();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ListBoxLang_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            lang = ListBoxLang.SelectedIndex;
            if (ListBoxLang.SelectedIndex != (int)ParentWindow.Lang)
                WarningBox.Visibility = System.Windows.Visibility.Visible;  
            else
                WarningBox.Visibility = System.Windows.Visibility.Visible;
        }
    }
}
