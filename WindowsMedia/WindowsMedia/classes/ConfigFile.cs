﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsMedia.classes;
using Newtonsoft.Json;
using System.Windows;

namespace WindowsMedia
{
    class ConfigFileData
    {
        private int height = 600;
        private int width = 800;
        private int volume = 50;

        public int Height
        {
            get { return height; }
            set { height = (value <= 340) ? 340 : value; }
        }
        public int Width
        {
            get { return width; }
            set { width = (value <= 480) ? 480 : value; }
        }
        public int Volume
        {
            get { return volume; }
            set { volume = (value <= 0) ? 0 : ((value >= 100) ? 100 : value); }
        }
        public bool Muted = false;
        public bool Shuffle = false;
        public bool Repeat = false;
        public MusicStyle Style = MusicStyle.ALBUM;
        public List<String> BiblioFiles = new List<String>();
        public WindowState WinState = WindowState.Normal;
        public Language Lang = Language.FRENCH;
    }

    class ConfigFile
    {
        private static ConfigFile instance = null;
        private static readonly object Lock = new object();
        private static String ConfigPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MediaStation", "conf.json");
        public ConfigFileData Data = null;

        public static ConfigFile Instance
        {
            get
            {
                lock (Lock)
                {
                    if (instance == null)
                    {
                        instance = new ConfigFile();
                    }
                    return instance;
                }
            }
        }

        private ConfigFile()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ConfigPath));
            if (File.Exists(ConfigPath))
                Data = JsonConvert.DeserializeObject<ConfigFileData>(File.ReadAllText(ConfigPath));
            else
            {
                Data = new ConfigFileData();
                Write();
            }
        }

        public void Write()
        {
            var data = JsonConvert.SerializeObject(Data);
            var f = File.Open(ConfigPath, FileMode.Create, FileAccess.Write);
            try
            {
                Byte[] info = new UTF8Encoding(true).GetBytes(data);
                f.Write(info, 0, info.Length);
            }
            finally
            {
                f.Close();
            }
        }
    }
}
