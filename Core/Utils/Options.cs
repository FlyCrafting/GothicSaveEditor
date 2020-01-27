using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Input;
using GothicSaveEditor.Core.Services;

namespace GothicSaveEditor.Core.Utils
{
    public static class Options
    {
        #region Parameters
        //Selected langauge
        private static string _language; // default value set(detected by OS language) in App.Xaml.cs
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

        //Is AutoBackup enabled?
        private static bool _keepBackups = true;
        public static bool KeepBackups
        {
            get => _keepBackups;
            set
            {
                _keepBackups = value;
                Save();
            }
        }
        #endregion

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();


        #region App main settings

        public static int InfoLinePopUpTime = 1500;

        public static readonly string ScriptsDirectory = @"\scripts";
        
        public static string GseVersion = "Gothic Save Editor v. " + Assembly.GetExecutingAssembly().GetName().Version.ToString().Remove(5) + " open-beta";

        private static readonly string SettingsFile = @"settings.gsec";
        #endregion

        static Options()
        {
            var loadState = Load();
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
            if (!loadState)
                MessageBox.Show(ResourceServices.GetString("UnableToLoadSettings"));
        }


        private static readonly Dictionary<string, Action<string>> Settings = new Dictionary<string, Action<string>>()
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
            ["keep_backups"] = val =>
            {
                if (val == "True")
                    _keepBackups = true;
                //Else do not change anything, false is by default
            }
        };

        public static bool Load()
        {
            //If file doesn't exists then do not try to load settings!
            if (!File.Exists(SettingsFile))
                return true;
            try
            {
                foreach (var line in File.ReadAllLines(SettingsFile))
                {
                    var lineSplitted = line.Split('=');
                    var val = line.Substring(lineSplitted[0].Length + 1).Trim();
                    Settings[lineSplitted[0].Trim().ToLower()](val);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return false;
            }

            return true;
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
                
                sb.AppendLine($"keep_backups={_keepBackups}");

                File.WriteAllText(SettingsFile, sb.ToString());
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                MessageBox.Show(ResourceServices.GetString("UnableToSaveSettings"));
            }
        }


    }
}
