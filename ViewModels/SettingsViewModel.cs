using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using GothicSaveEditor.Core.Primitives;
using GothicSaveEditor.Core.Services;
using GothicSaveEditor.Core.Utils;
using GothicSaveEditor.Models;

namespace GothicSaveEditor.ViewModels
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
            get => Options.GamePath;
            set
            {
                Options.GamePath = value;
                OnPropertyChanged();
            }
        }

        public string CurrentLanguage
        {
            get => Options.Language;
            set
            {
                Options.Language = value;
                App.SetLanguage(value);
                OnPropertyChanged();
            }
        }

        public bool KeepBackups
        {
            get => Options.KeepBackups;
            set
            {
                Options.KeepBackups = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<string> Languages { get; set; } = new ObservableCollection<string> { "English", "Русский" };
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
                        var path = FileService.PickGothicFolder();
                        if (path == null)
                            return;
                        PathLine = path;
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                        MessageBox.Show(ResourceServices.GetString("UnableToSelectGameFolder"));
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

        public RelayCommand DeleteBackupsCommand
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    if (MessageBox.Show(ResourceServices.GetString("AgreeBackupsDeleting"),
                            ResourceServices.GetString("Warning"), MessageBoxButton.YesNo, MessageBoxImage.Warning) ==
                        MessageBoxResult.Yes)
                    {
                        if (!BackupService.DeleteAll())
                        {
                            MessageBox.Show(ResourceServices.GetString("CouldNotDeleteBackups"));
                        }
                    }
                }, b => BackupService.BackupsCount > 0);
            }
        }
    }
}
