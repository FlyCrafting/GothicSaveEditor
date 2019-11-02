using GothicSaveEditor.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace GothicSaveEditor
{
    public static class Settings
    {
        #region Parameters
        //Selected langauge
        private static string language = null;
        public static string Language
        {
            get => language;
            set
            {
                language = value;
                Save();
            }
        }

        //Path to the gothic folder
        private static string gamePath = "";
        public static string GamePath
        {
            get => gamePath;
            set
            {
                gamePath = value;
                Save();
            }
        }

        //Is Auto-Search enabled?
        private static bool autoSearch = true;
        public static bool AutoSearch
        {
            get => autoSearch;
            set
            {
                autoSearch = value;
                Save();
            }
        }

        //Is AutoBackup enabled?
        private static bool autoBackup = false;
        public static bool AutoBackup
        {
            get => autoBackup;
            set
            {
                autoBackup = value;
                Save();
            }
        }
        #endregion

        #region App main settings

        public static int InfoLinePopUpTime = 1500;

        public static readonly string scriptsDirectory = @"\scripts";
        
        public static string GSEVersion = "Gothic Save Editor v " + Assembly.GetExecutingAssembly().GetName().Version.ToString().Remove(5);

        private static readonly string settingsFile = @"settings.gsec";

        private static bool _exceptionDuringLoading = false;
        #endregion

        static Settings()
        {
            Load();
            if (language == null)
            {
                foreach (var curLang in InputLanguageManager.Current.AvailableInputLanguages)
                {
                    if (curLang.ToString() == "ru-RU")
                    {
                        App.SetLanguage(curLang.ToString());
                    }
                }
            }
            else
            {
                App.SetLanguage(language);
            }
            if (_exceptionDuringLoading)
                MessageBox.Show(ResourceService.GetString("UnableToLoadSettings"));
        }


        static readonly Dictionary<string, Action<string>> settings = new Dictionary<string, Action<string>>()
        {
            ["language"] = val =>
            {
                if (val == "Русский")
                    language = "Русский";
                else if (val == "English")
                {
                    language = "English";
                }
                //Else use system language
            },
            ["game_path"] = val => gamePath = val,
            ["auto_search"] = val =>
            {
                if (val == "false")
                    autoSearch = false;
                //Else do not change anything, true is by default
            },
            ["auto_backup"] = val =>
            {
                if (val == "true")
                    autoBackup = true;
                //Else do not change anything, false is by default
            }
        };

        public static void Load()
        {
            //If file doesn't exists then do not try to load settings!
            if (!File.Exists(settingsFile))
                return;
            try
            {
                foreach (var line in File.ReadAllLines(settingsFile))
                {
                    var lineSplitted = line.Split('=');
                    var val = line.Substring(lineSplitted[0].Length + 1).Trim();
                    settings[lineSplitted[0].Trim().ToLower()](val);
                }
            }
            catch (Exception ex)
            {
                _exceptionDuringLoading = true;
                Logger.Log(ex);
            }
        }

        public static void Save()
        {
            try
            {
                StringBuilder sb = new StringBuilder();

                if (language != null)
                    sb.AppendLine($"language={language}");

                if (gamePath != null)
                    sb.AppendLine($"game_path={gamePath}");

                sb.AppendLine($"auto_search={autoSearch}");

                sb.AppendLine($"auto_backup={autoBackup}");

                File.WriteAllText(settingsFile, sb.ToString());
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                MessageBox.Show(ResourceService.GetString("UnableToSaveSettings"));
            }
        }


    }
}
