﻿using GothicSaveEditor.Models;
using GothicSaveEditor.Services;
using GothicSaveTools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GothicSaveEditor.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        #region PropertyChanged (for binding)
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
        #endregion

        #region Properties
        private string _searchLine;
        public string SearchLine
        {
            get => _searchLine;
            set
            {
                _searchLine = value;
                if (AutoSearch && !SaveGameNull())
                {
                    Search();
                }
                OnPropertyChanged();
            }
        }

        private string _leftInfoLine;
        public string LeftInfoLine
        {
            get => _leftInfoLine;
            set
            {
                _leftInfoLine = value;
                OnPropertyChanged();
            }
        }

        private string _rightInfoLine;
        public string RightInfoLine
        {
            get => _rightInfoLine;
            set
            {
                _rightInfoLine = value;
                OnPropertyChanged();
            }
        }


        public bool AutoSearch
        {
            get => Settings.AutoSearch;
            set
            {
                Settings.AutoSearch = value;
                if (value && !SaveGameNull())
                {
                    Search();
                }
            }
        }
        #endregion

        #region Variables
        public ObservableCollection<GothicVariable> DataGridVariables { get; set; } = new ObservableCollection<GothicVariable>();
        public ObservableCollection<Script> Scripts { get; set; } = new ObservableCollection<Script>();

        private SaveGame? openedSaveGame = null;

        private bool dynamicInfo = false;

        public string GSEVersion
        {
            get => Settings.GSEVersion;
        }

        #endregion

        #region Commands
        public RelayCommand OpenCommand
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    string saveGamePath = null;
                    try
                    {
                        //Trying to get savegame path using windows dialog.
                        saveGamePath = FileService.ImportSave();
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(ex);
                        MessageBox.Show(ResourceService.GetString("UnableToLoadSavegameLoadError"));
                    }
                    //If user didn't select path - return.
                    if (saveGamePath == null)
                        return;
                    if (saveGamePath.Trim().Length == 0 || !File.Exists(saveGamePath))
                    {
                        MessageBox.Show(ResourceService.GetString("UnableToLoadSavegameWrongPath"));
                    }
                    LeftInfoLine = ResourceService.GetString("LoadingSaveGame");
                    DataGridVariables.Clear();
                    Task.Run(() => LoadSaveGame(saveGamePath)).ContinueWith(a=>LoadScripts());
                }, a => SaveGameNull());
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
                        if (!IsDataGridChanged())
                        {
                            SetDynamicInfo("NoNeedToSave");
                            return;
                        }
                        if (Settings.AutoBackup)
                        {
                            File.Copy(openedSaveGame.Value.FilePath, openedSaveGame.Value.FilePath + ".bak", true);
                        }
                        WriteVariablesToFile();
                        SetDynamicInfo("SavedSucessfully");
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(ex);
                        MessageBox.Show(ResourceService.GetString("UnableToSaveSavegame"));
                    }
                }, a=>!SaveGameNull());
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
                            File.Copy(openedSaveGame.Value.FilePath, path, true);
                        }
                        catch (Exception ex)
                        {
                            if (ex.HResult == -2147024864)
                            {
                                Logger.Log(ex);
                                MessageBox.Show(ResourceService.GetString("UnableToSaveSavegameProcess"));
                            }
                        }
                        WriteVariablesToFile(path);
                        SetDynamicInfo("SavedSucessfully");
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(ex);
                        MessageBox.Show(ResourceService.GetString("UnableToSaveSavegame"));
                    }
                }, a => !SaveGameNull());
            }
        }

        public RelayCommand SearchCommand
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    try
                    {
                        Search();
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(ex);
                    }
                }, a => !SaveGameNull());
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
                        Logger.Log(ex);
                    }
                });
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
                        Logger.Log(ex);
                    }
                });
            }
        }

        public RelayCommand CloseSavegameCommand
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    try
                    {
                        if(!IsDataGridChanged() || MessageBox.Show(ResourceService.GetString("SavegameWasModified"), ResourceService.GetString("Warning"), MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                        {
                            DataGridVariables.Clear();
                            Scripts.Clear();
                            openedSaveGame = null;
                            LeftInfoLine = "";
                            RightInfoLine = "";
                            SearchLine = "";
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(ex);
                    }
                }, a => !SaveGameNull());
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
                        File.Copy(openedSaveGame.Value.FilePath, openedSaveGame.Value.FilePath + ".bak", true);
                        SetDynamicInfo("BackupCreated");
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(ex);
                        MessageBox.Show(ResourceService.GetString("UnableToBackup"));
                    }
                }, a => !SaveGameNull());
            }
        }

        //Not useful command, only for checking ImportScript availability
        public RelayCommand ImportVariablesCommand => new RelayCommand(obj =>{}, a => !SaveGameNull());

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
                        LeftInfoLine = ResourceService.GetString("ExportingVariables");
                        Task.Run(() => ExportVariablesTask(path));
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(ex);
                        MessageBox.Show(ResourceService.GetString("UnableToExportVariables"));
                    }
                }, a => !SaveGameNull());
            }
        }

        #endregion


        private void ExportVariablesTask(string path)
        {
            var writeText = new StringBuilder();
            foreach (var variable in openedSaveGame.Value.VariablesList)
            {
                writeText.AppendLine(variable.FullName + " = " + variable.Value);
            }
            File.WriteAllText(path, writeText.ToString());
            SetDynamicInfo("VariablesExportedSucessfully", true);
        }

        private void ImportVariables(Script script)
        {
            try
            {
                LeftInfoLine = ResourceService.GetString("ImportingVariables");
                var changesList = new StringBuilder();
                var notFoundVariables = new StringBuilder();
                var fastVariables = new Dictionary<string, GothicVariable>();
                foreach(var variable in openedSaveGame.Value.VariablesList)
                {
                    fastVariables[variable.FullName.ToLower()] = variable;
                }
                foreach (var item in script.actions)
                {
                    try
                    {
                        fastVariables[item.Key.ToLower()].Value = item.Value;
                        if (fastVariables[item.Key.ToLower()].Modified)
                        {
                            changesList.AppendLine("\n" + item.Key + " -> " + item.Value);
                        }
                    }
                    catch
                    {
                        notFoundVariables.AppendLine(item.Key);
                    }
                }

                DispatchService.Invoke(new Action(() =>
                {
                    DataGridVariables.Clear();
                    foreach (var p in openedSaveGame.Value.VariablesList)
                        DataGridVariables.Add(p);
                }));
                SearchLine = "";
                if (changesList.Length != 0)
                {
                    MessageBox.Show(ResourceService.GetString("VariablesWereImported") + "\n\n" + ResourceService.GetString("ScriptActionList") + changesList);
                }
                else
                {
                    MessageBox.Show(ResourceService.GetString("VariablesWereNotImported"));
                }
                if (notFoundVariables.Length > 0)
                {
                    MessageBox.Show(ResourceService.GetString("NotFoundVariables") + notFoundVariables);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                MessageBox.Show(ResourceService.GetString("UnableToImportVariables"));
            }
            finally
            {
                LeftInfoLine = openedSaveGame.Value.FilePath;
            }
        }

        private void LoadScripts()
        {
            try
            {
                if (!Directory.Exists(Environment.CurrentDirectory + Settings.scriptsDirectory))
                {
                    Directory.CreateDirectory(Environment.CurrentDirectory + Settings.scriptsDirectory);
                    return;
                }
                string[] dirs = Directory.GetFiles(Environment.CurrentDirectory + Settings.scriptsDirectory, "*.gses");
                if (dirs.Length == 0)
                {
                    return;
                }
                DispatchService.Invoke(new Action(() =>
                {
                    Scripts.Clear();
                }));
                foreach (var dir in dirs)
                {
                    //This try made to prevent stopping loading scripts if one script crashed
                    try
                    {
                        string[] lines = File.ReadAllLines(dir);
                        if (lines.Length == 0)
                            continue;
                        var nameVal = new Dictionary<string, int>();
                        foreach (var line in lines)
                        {
                            if (String.IsNullOrWhiteSpace(line))
                                continue;
                            int number = line.IndexOf("=");
                            if (number < 1)
                                continue;
                            string varName = line.Substring(0, number).Replace(" ", "");
                            string value = line.Substring(number + 1).Replace(" ", "");
                            int varValue = Int32.Parse(value);
                            nameVal[varName] = varValue;
                        }
                        DispatchService.Invoke(new Action(() =>
                        {
                            Scripts.Add(new Script(Path.GetFileNameWithoutExtension(dir), nameVal, ImportVariables));
                        }));
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(ex);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                MessageBox.Show(ResourceService.GetString("UnableToLoadScripts"));
            }
        }

        private void SetDynamicInfo(string text, bool setToPath=false)
        {
            if (dynamicInfo)
                return;
            dynamicInfo = true;
            string backup = "";
            if (setToPath && !SaveGameNull())
            {
                backup = openedSaveGame.Value.FilePath;
            }
            else
            {
                backup = LeftInfoLine;
            }
            LeftInfoLine = ResourceService.GetString(text);
            Task.Delay(Settings.infoLinePopUpTime).ContinueWith(t => ResetDynamicInfo(backup));
        }

        private void ResetDynamicInfo(string toText)
        {
            LeftInfoLine = toText;
            dynamicInfo = false;
        }

        private void LoadSaveGame(string saveGamePath)
        {
            //Path is always not null here!
            try
            {
                List<GothicVariable> tempVariables = new SaveParser().Parse(saveGamePath);
                if (tempVariables == null)
                    return;
                openedSaveGame = new SaveGame(saveGamePath, tempVariables);

                DispatchService.Invoke(new Action(() =>
                {
                    foreach (var p in openedSaveGame.Value.VariablesList)
                        DataGridVariables.Add(p);
                }));
                LeftInfoLine = openedSaveGame.Value.FilePath;
                RightInfoLine = openedSaveGame.Value.VariablesList.Count.ToString();
            }
            catch(Exception ex)
            {
                Logger.Log(ex);
                MessageBox.Show(ResourceService.GetString("UnableToLoadSavegameUnknownError"));
                SetDynamicInfo("UnableToLoadSavegame");
            }
        }

        private void WriteVariablesToFile(string path = null)
        {
            if (path == null)
            {
                path = openedSaveGame.Value.FilePath;
            }
            //No try here. It's handled before.
            DispatchService.Invoke(new Action(() =>
            {
                FileStream fstr = new FileStream(path, FileMode.Open, FileAccess.Write);
                BinaryWriter w = new BinaryWriter(fstr);
                for (int i = 0; i < DataGridVariables.Count; i++)
                {
                    if (DataGridVariables[i].Modified)
                    {
                        w.Seek(DataGridVariables[i].Position, 0);
                        w.Write(DataGridVariables[i].Value);
                        DataGridVariables[i].Saved();
                    }
                }
                w.Close();
                fstr.Close();
            }));
        }

        private bool SaveGameNull()
        {
            return openedSaveGame == null;
        }

        private bool IsDataGridChanged()
        {
            bool isChanged = false;
            DispatchService.Invoke(new Action(() =>
            {
                for (int i = 0; i < DataGridVariables.Count; i++)
                {
                    if (DataGridVariables[i].Modified)
                    {
                        isChanged = true;
                    }
                }
            }));
            return isChanged;
        }


        private void Search()
        {
            if (SaveGameNull())
                return;
            try
            {
                var variables = SearchService.Search(SearchLine, openedSaveGame.Value.VariablesList);
                DispatchService.Invoke(new Action(() =>
                {
                    DataGridVariables.Clear();
                    foreach (var p in variables)
                        DataGridVariables.Add(p);
                }));
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                MessageBox.Show(ResourceService.GetString("UnableToSearch"));
            }
        }
    }
}