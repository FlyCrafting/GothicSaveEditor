using GothicSaveEditor.Models;
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
using NLog;

namespace GothicSaveEditor.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
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
                if (!SaveGameNull())
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


        /*public bool AutoSearch
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
        }*/
        #endregion

        #region Variables
        public ObservableCollection<GothicVariable> DataGridVariables { get; set; } = new ObservableCollection<GothicVariable>();
        public ObservableCollection<Script> Scripts { get; set; } = new ObservableCollection<Script>();

        private SaveGame? _openedSaveGame;

        private bool _dynamicInfo;

        // ReSharper disable once UnusedMember.Global
        public string GseVersion => Settings.GseVersion;

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
                        Logger.Error(ex);
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
                            File.Copy(_openedSaveGame.Value.FilePath, _openedSaveGame.Value.FilePath + ".bak", true);
                        }
                        WriteVariablesToFile();
                        SetDynamicInfo("SavedSucessfully");
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
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
                            File.Copy(_openedSaveGame.Value.FilePath, path, true);
                        }
                        catch (Exception ex)
                        {
                            if (ex.HResult == -2147024864)
                            {
                                Logger.Error(ex);
                                MessageBox.Show(ResourceService.GetString("UnableToSaveSavegameProcess"));
                            }
                        }
                        WriteVariablesToFile(path);
                        SetDynamicInfo("SavedSucessfully");
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
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
                        Logger.Error(ex);
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
                        Logger.Error(ex);
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
                        Logger.Error(ex);
                    }
                });
            }
        }

        public RelayCommand CloseCommand
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
                            _openedSaveGame = null;
                            LeftInfoLine = "";
                            RightInfoLine = "";
                            SearchLine = "";
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
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
                        File.Copy(_openedSaveGame.Value.FilePath, _openedSaveGame.Value.FilePath + ".bak", true);
                        SetDynamicInfo("BackupCreated");
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                        MessageBox.Show(ResourceService.GetString("UnableToBackup"));
                    }
                }, a => !SaveGameNull());
            }
        }

        //Not useful command, only for checking ImportScript availability
        public RelayCommand ApplyScriptCommands => new RelayCommand(obj =>{}, a => !SaveGameNull() && Scripts.Count > 0);

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
                        Logger.Error(ex);
                        MessageBox.Show(ResourceService.GetString("UnableToExportVariables"));
                    }
                }, a => !SaveGameNull());
            }
        }

        #endregion


        private void ExportVariablesTask(string path)
        {
            var writeText = new StringBuilder();
            foreach (var variable in _openedSaveGame.Value.VariablesList)
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
                foreach(var variable in _openedSaveGame.Value.VariablesList)
                {
                    fastVariables[variable.FullName.ToLower()] = variable;
                }
                foreach (var item in script.actions)
                {
                    if (fastVariables.ContainsKey(item.Key.ToLower()))
                    {
                        fastVariables[item.Key.ToLower()].Value = item.Value;
                        changesList.AppendLine("\n" + item.Key + " -> " + item.Value);
                    }
                    else
                    {
                        notFoundVariables.AppendLine(item.Key);
                    }
                }

                DispatchService.Invoke(new Action(() =>
                {
                    DataGridVariables.Clear();
                    foreach (var p in _openedSaveGame.Value.VariablesList)
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
                Logger.Error(ex);
                MessageBox.Show(ResourceService.GetString("UnableToImportVariables"));
            }
            finally
            {
                LeftInfoLine = _openedSaveGame.Value.FilePath;
            }
        }

        private void LoadScripts()
        {
            try
            {
                if (!Directory.Exists(Environment.CurrentDirectory + Settings.ScriptsDirectory))
                {
                    Directory.CreateDirectory(Environment.CurrentDirectory + Settings.ScriptsDirectory);
                    return;
                }
                string[] dirs = Directory.GetFiles(Environment.CurrentDirectory + Settings.ScriptsDirectory, "*.gses");
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
                        if (lines.Length == 0 || lines.Length > 250)
                            continue;
                        var nameVal = new Dictionary<string, int>();
                        foreach (var line in lines)
                        {
                            if (string.IsNullOrWhiteSpace(line))
                                continue;
                            int number = line.IndexOf("=");
                            if (number < 1)
                                continue;
                            string varName = line.Substring(0, number).Replace(" ", "");
                            string value = line.Substring(number + 1).Replace(" ", "");
                            int varValue = int.Parse(value);
                            nameVal[varName] = varValue;
                        }
                        DispatchService.Invoke(new Action(() =>
                        {
                            Scripts.Add(new Script(Path.GetFileNameWithoutExtension(dir), nameVal, ImportVariables));
                        }));
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                MessageBox.Show(ResourceService.GetString("UnableToLoadScripts"));
            }
        }

        private void SetDynamicInfo(string text, bool setToPath=false)
        {
            if (_dynamicInfo)
                return;
            _dynamicInfo = true;
            string backup = "";
            if (setToPath && !SaveGameNull())
            {
                backup = _openedSaveGame.Value.FilePath;
            }
            else
            {
                backup = LeftInfoLine;
            }
            LeftInfoLine = ResourceService.GetString(text);
            Task.Delay(Settings.InfoLinePopUpTime).ContinueWith(t => ResetDynamicInfo(backup));
        }

        private void ResetDynamicInfo(string toText)
        {
            LeftInfoLine = toText;
            _dynamicInfo = false;
        }

        private void LoadSaveGame(string saveGamePath)
        {
            //Path is always not null here!
            try
            {
                List<GothicVariable> tempVariables = new SaveReader().Read(saveGamePath);
                if (tempVariables == null)
                    return;
                _openedSaveGame = new SaveGame(saveGamePath, tempVariables);

                DispatchService.Invoke(new Action(() =>
                {
                    foreach (var p in _openedSaveGame.Value.VariablesList)
                        DataGridVariables.Add(p);
                }));
                LeftInfoLine = _openedSaveGame.Value.FilePath;
                RightInfoLine = _openedSaveGame.Value.VariablesList.Count.ToString();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                MessageBox.Show("[SaveReader]" + ResourceService.GetString(ex.Message));
                SetDynamicInfo("UnableToLoadSavegame");
            }
        }

        private void WriteVariablesToFile(string path = null)
        {
            if (path == null)
            {
                path = _openedSaveGame.Value.FilePath;
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
            return _openedSaveGame == null;
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
                var variables = SearchService.Search(SearchLine, _openedSaveGame.Value.VariablesList);
                DispatchService.Invoke(new Action(() =>
                {
                    DataGridVariables.Clear();
                    foreach (var p in variables)
                        DataGridVariables.Add(p);
                }));
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                MessageBox.Show(ResourceService.GetString("UnableToSearch"));
            }
        }
    }
}