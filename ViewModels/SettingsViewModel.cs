using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using GothicSaveEditor.Core.Primitives;
using GothicSaveEditor.Core.HelpServices;
using GothicSaveEditor.Core.Utils;

namespace GothicSaveEditor.ViewModel
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        #region PropertyChanged (for binding)
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
        #endregion
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public string PathLine
        {
            get => Settings.GamePath;
            set
            {
                Settings.GamePath = value;
                OnPropertyChanged();
            }
        }

        public string CurrentLanguage
        {
            get => Settings.Language;
            set
            {
                Settings.Language = value;
                App.SetLanguage(value);
                OnPropertyChanged();
            }
        }

        public bool BackupBeforeSaving
        {
            get => Settings.AutoBackup;
            set
            {
                Settings.AutoBackup = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<string> Languages { get; set; } = new ObservableCollection<string>() { "English", "Русский" };
        private readonly SettingsWindow _settingsWindow;

        public SettingsViewModel(SettingsWindow settingsWindow)
        {
            _settingsWindow = settingsWindow;
        }

        public RelayCommand SelectFolderCommand
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    try
                    {
                        var path = FileManager.PickGothicFolder();
                        if (path == null)
                            return;
                        PathLine = path;
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                        MessageBox.Show(ResourceManager.GetString("UnableToSelectGameFolder"));
                    }
                });
            }
        }


        public RelayCommand CloseWindowCommand
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    _settingsWindow.Close();
                });
            }
        }
    }
}
