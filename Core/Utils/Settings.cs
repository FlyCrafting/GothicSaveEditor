using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Input;
using GothicSaveEditor.Core.HelpServices;

namespace GothicSaveEditor.Core.Utils
{
    public static class Settings
    {
        #region Parameters
        //Selected langauge
        private static string _language;
        public static string Language
        {
            get => _language;
            set
            {
                _language = value;
                Save();
            }
        }

        //Path to the gothic folder
        private static string _gamePath = "";
        public static string GamePath
        {
            get => _gamePath;
            set
            {
                _gamePath = value;
                Save();
            }
        }

        //Is Auto-Search enabled?
        /*private static bool _autoSearch = true;
        public static bool AutoSearch
        {
            get => _autoSearch;
            set
            {
                _autoSearch = value;
                Save();
            }
        }*/

        //Is AutoBackup enabled?
        private static bool _autoBackup = false;
        public static bool AutoBackup
        {
            get => _autoBackup;
            set
            {
                _autoBackup = value;
                Save();
            }
        }
        #endregion

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();


        #region App main settings

        public static int InfoLinePopUpTime = 1500;

        public static readonly string ScriptsDirectory = @"\scripts";
        
        public static string GseVersion = "Gothic Save Editor v. " + Assembly.GetExecutingAssembly().GetName().Version.ToString().Remove(5) + " closed-beta";

        private static readonly string SettingsFile = @"settings.gsec";

        private static bool _exceptionDuringLoading = false;
        #endregion

        static Settings()
        {
            Load();
            if (_language == null)
            {
                if (InputLanguageManager.Current.AvailableInputLanguages != null)
                    foreach (var curLang in InputLanguageManager.Current.AvailableInputLanguages)
                    {
                        if (curLang.ToString() == "ru-RU")
                        {
                            App.SetLanguage("Русский");
                        }
                    }
            }
            else
            {
                App.SetLanguage(_language);
            }
            if (_exceptionDuringLoading)
                MessageBox.Show(ResourceManager.GetString("UnableToLoadSettings"));
        }


        private static readonly Dictionary<string, Action<string>> settings = new Dictionary<string, Action<string>>()
        {
            ["language"] = val =>
            {
                if (val == "Russian")
                    _language = "Русский";
                else if (val == "English")
                {
                    _language = "English";
                }
                //Else use system language
            },
            ["game_path"] = val => _gamePath = val,
            /*["auto_search"] = val =>
            {
                if (val == "false")
                    _autoSearch = false;
                //Else do not change anything, true is by default
            },*/
            ["auto_backup"] = val =>
            {
                if (val == "true")
                    _autoBackup = true;
                //Else do not change anything, false is by default
            }
        };

        public static void Load()
        {
            //If file doesn't exists then do not try to load settings!
            if (!File.Exists(SettingsFile))
                return;
            try
            {
                foreach (var line in File.ReadAllLines(SettingsFile))
                {
                    var lineSplitted = line.Split('=');
                    var val = line.Substring(lineSplitted[0].Length + 1).Trim();
                    settings[lineSplitted[0].Trim().ToLower()](val);
                }
            }
            catch (Exception ex)
            {
                _exceptionDuringLoading = true;
                Logger.Error(ex);
            }
        }

        public static void Save()
        {
            try
            {
                var sb = new StringBuilder();

                if (_language != null)
                {
                    sb.AppendLine(_language == "Русский" ? "language=Russian" : $"language={_language}");
                }

                if (_gamePath != null)
                    sb.AppendLine($"game_path={_gamePath}");

                //sb.AppendLine($"auto_search={_autoSearch}");

                sb.AppendLine($"auto_backup={_autoBackup}");

                File.WriteAllText(SettingsFile, sb.ToString());
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                MessageBox.Show(ResourceManager.GetString("UnableToSaveSettings"));
            }
        }


    }
}
