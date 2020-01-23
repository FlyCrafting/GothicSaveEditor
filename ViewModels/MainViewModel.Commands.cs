using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using GothicSaveEditor.Core.Services;
using GothicSaveEditor.Core.Utils;
using GothicSaveEditor.Models;

namespace GothicSaveEditor.ViewModels
{
    public partial class MainViewModel
    {
        public RelayCommand OpenCommand
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    if (!IsSaveNull && !CanCloseSave)
                        return;

                    string saveGamePath = null;
                    try
                    {
                        //Trying to get savegame path using windows dialog.
                        saveGamePath = FileService.ImportSave();
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                        MessageBox.Show(ResourceServices.GetString("UnableToLoadSavegameLoadError"));
                    }
                    //If user didn't select path - return.
                    if (saveGamePath == null)
                        return;
                    
                    if (saveGamePath.Trim().Length == 0 || !File.Exists(saveGamePath))
                    {
                        MessageBox.Show(ResourceServices.GetString("UnableToLoadSavegameWrongPath"));
                        return;
                    }
                    ClearWorkSpace();
                    LeftInfoLine = ResourceServices.GetString("LoadingSaveGame");
                    Task.Run(() => LoadSaveGame(saveGamePath)); //.ContinueWith(a=>LoadScripts());
                }, a => true);
            }
        }


        public RelayCommand SaveCommand
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    try
                    {
                        if (Settings.AutoBackup)
                        {
                            File.Copy(_openedSaveGame.Value.FilePath, _openedSaveGame.Value.FilePath + ".bak", true);
                        }
                        WriteVariablesToFile(_openedSaveGame.Value.FilePath);
                        SetDynamicInfo("SavedSucessfully");
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                        MessageBox.Show(ResourceServices.GetString("UnableToSaveSavegame"));
                    }
                }, a=> IsSaveModified);
            }
        }

        public RelayCommand SaveAsCommand
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    try
                    {
                        string path = FileService.ExportSave();
                        if (path == null)
                            return;
                        try
                        {
                            File.Copy(_openedSaveGame.Value.FilePath, path, true);
                        }
                        catch (Exception ex)
                        {
                            if (ex.HResult == -2147024864)
                            {
                                Logger.Error(ex);
                                MessageBox.Show(ResourceServices.GetString("UnableToSaveSavegameProcess"));
                            }
                        }
                        WriteVariablesToFile(path);
                        SetDynamicInfo("SavedSucessfully");
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                        MessageBox.Show(ResourceServices.GetString("UnableToSaveSavegame"));
                    }
                }, a => !IsSaveNull);
            }
        }

        public RelayCommand CloseCommand
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    if (CanCloseSave)
                        ClearWorkSpace();
                }, a => !IsSaveNull);
            }
        }


        public RelayCommand SettingsCommand
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    try
                    {
                        WindowsService.OpenSettings();
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                    }
                });
            }
        }

        public RelayCommand MakeBackupCommand
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    try
                    {
                        File.Copy(_openedSaveGame.Value.FilePath, _openedSaveGame.Value.FilePath + ".bak", true);
                        SetDynamicInfo("BackupCreated");
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                        MessageBox.Show(ResourceServices.GetString("UnableToBackup"));
                    }
                }, a => !IsSaveNull);
            }
        }

        public RelayCommand AboutCommand
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    try
                    {
                        WindowsService.OpenAbout();
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                    }
                });
            }
        }
        
        //Not useful command, only for checking ImportScript availability
        public RelayCommand ApplyScriptCommands => new RelayCommand(obj =>{}, a => !IsSaveNull && Scripts.Count > 0);

        public RelayCommand ExportVariablesCommand
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    try
                    {
                        string path = FileService.ExportVariables();
                        if (path == null)
                            return;
                        LeftInfoLine = ResourceServices.GetString("ExportingVariables");
                        Task.Run(() => ExportVariablesTask(path));
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                        MessageBox.Show(ResourceServices.GetString("UnableToExportVariables"));
                    }
                }, a => !IsSaveNull);
            }
        }
    }
}