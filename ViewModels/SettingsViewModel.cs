using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using GothicSaveEditor.Services;
using GothicSaveTools.Properties;

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
        private SettingsWindow settingsWindow;

        public SettingsViewModel(SettingsWindow settingsWindow)
        {
            this.settingsWindow = settingsWindow;
        }

        public RelayCommand SelectFolderCommand
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    try
                    {
                        string path = FileService.PickGothicFolder();
                        if (path == null)
                            return;
                        PathLine = path;
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(ex);
                        MessageBox.Show(ResourceService.GetString("UnableToSelectGameFolder"));
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
                    settingsWindow.Close();
                });
            }
        }
    }
}
