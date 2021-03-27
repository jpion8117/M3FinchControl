using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M3FinchControl
{
    static class ThemeManager
    {
        public enum Themes
        {
            Classic, //white on black
            Fallout, //dark yellow on black
            BSOD, //white on dark blue
            ET_Game, //dark green on green
            Zelda, // dark green on yellow
            Grayscale //dark gray on gray
        }

        static public void LoadTheme()
        {
            // *************
            // * Variables *
            // *************
            string[] SettingsFile;

            // ***************
            // * File Loader *
            // ***************
            try
            {
                SettingsFile = System.IO.File.ReadAllLines(SETTINGS_FILE);
            }
            catch (System.IO.FileNotFoundException)
            {
                //save the default theme to the file
                System.IO.File.WriteAllText(SETTINGS_FILE, "Theme:Classic");

                //give the loader the data to work with
                SettingsFile = new string[] { "Theme:Classic" };
            }

            // ********************
            // * Extract the data *
            // ********************
            foreach (string setting in SettingsFile)
            {
                if (setting.Contains("Theme:"))
                {
                    string themeName = "";

                    for (int index = setting.IndexOf(':') + 1; index < setting.Length; ++index)
                    {
                        themeName += setting[index];
                    }

                    if (Enum.TryParse(themeName, out theme))
                    {
                        SetTheme(themeName);
                    }                    
                }
            }
        }

        static public void saveTheme()
        {
            // *************
            // * Variables *
            // *************
            string[] SettingsFile;

            // ***************
            // * File Loader *
            // ***************
            try
            {
                SettingsFile = System.IO.File.ReadAllLines(SETTINGS_FILE);
            }
            catch (System.IO.FileNotFoundException)
            {
                //save the default theme to the file
                System.IO.File.WriteAllText(SETTINGS_FILE, "Theme:Classic");

                //give the loader the data to work with
                SettingsFile = new string[] { "Theme:Classic" };
            }

            // *****************************
            // * Edit the data and save it *
            // *****************************
            for (int index = 0; index < SettingsFile.Length; index++) 
            {
                if (SettingsFile[index].Contains("Theme:"))
                {
                    SettingsFile[index] = $"Theme:{theme.ToString()}";
                    break;
                }
            }

            System.IO.File.WriteAllLines(SETTINGS_FILE, SettingsFile);
        }

        /// <summary>
        /// 
        /// </summary>
        static public void SetTheme(string themeName)
        {
            Themes localTheme;
            if (Enum.TryParse(themeName, out localTheme))
            {
                switch (localTheme)
                {
                    case Themes.Classic:
                        Menu.defaultForeground = ConsoleColor.White;
                        Menu.defaultBackground = ConsoleColor.Black;
                        break;
                    case Themes.Fallout:
                        Menu.defaultForeground = ConsoleColor.DarkYellow;
                        Menu.defaultBackground = ConsoleColor.Black;
                        break;
                    case Themes.BSOD:
                        Menu.defaultForeground = ConsoleColor.White;
                        Menu.defaultBackground = ConsoleColor.DarkBlue;
                        break;
                    case Themes.ET_Game:
                        Menu.defaultForeground = ConsoleColor.Green;
                        Menu.defaultBackground = ConsoleColor.DarkGreen;
                        break;
                    case Themes.Zelda:
                        Menu.defaultForeground = ConsoleColor.DarkGreen;
                        Menu.defaultBackground = ConsoleColor.Yellow;
                        break;
                    case Themes.Grayscale:
                        Menu.defaultForeground = ConsoleColor.DarkGray;
                        Menu.defaultBackground = ConsoleColor.Gray;
                        break;
                }
            }
        }

        static public Themes theme;// { private set; get; }
        public const string SETTINGS_FILE = "settings.conf";
    }
}
