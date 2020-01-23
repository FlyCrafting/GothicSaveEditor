using GothicSaveEditor.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using GothicSaveEditor.Core.Primitives;
using GothicSaveEditor.Core.Readers;
using GothicSaveEditor.Core.HelpServices;
using GothicSaveEditor.Core.Utils;
using NLog;

namespace GothicSaveEditor.ViewModels
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
        private bool IsSaveNull => _openedSaveGame == null;
        private bool IsSaveModified
        {
            get
            {
                if (IsSaveNull)
                    return false;
                var isChanged = false;
                Dispatcher.Invoke(() =>
                {
                    foreach (var t in DataGridVariables)
                    {
                        if (t.Modified)
                        {
                            isChanged = true;
                        }
                    }
                });
                return isChanged;
            }
        }

        private string _searchLine;
        public string SearchLine
        {
            get => _searchLine;
            set
            {
                _searchLine = value;
                if (!IsSaveNull)
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
                    if (!IsSaveNull)
                    {
                        CloseSavegame();
                    }
                    string saveGamePath = null;
                    try
                    {
                        //Trying to get savegame path using windows dialog.
                        saveGamePath = FileManager.ImportSave();
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                        MessageBox.Show(ResourceManager.GetString("UnableToLoadSavegameLoadError"));
                    }
                    //If user didn't select path - return.
                    if (saveGamePath == null)
                        return;
                    if (saveGamePath.Trim().Length == 0 || !File.Exists(saveGamePath))
                    {
                        MessageBox.Show(ResourceManager.GetString("UnableToLoadSavegameWrongPath"));
                    }
                    LeftInfoLine = ResourceManager.GetString("LoadingSaveGame");
                    DataGridVariables.Clear();
                    Task.Run(() => LoadSaveGame(saveGamePath)).ContinueWith(a=>LoadScripts());
                }, a => !IsSaveModified);
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
                        WriteVariablesToFile();
                        SetDynamicInfo("SavedSucessfully");
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                        MessageBox.Show(ResourceManager.GetString("UnableToSaveSavegame"));
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
                        string path = FileManager.ExportSave();
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
                                MessageBox.Show(ResourceManager.GetString("UnableToSaveSavegameProcess"));
                            }
                        }
                        WriteVariablesToFile(path);
                        SetDynamicInfo("SavedSucessfully");
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                        MessageBox.Show(ResourceManager.GetString("UnableToSaveSavegame"));
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
                    CloseSavegame();
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
                        WindowsManager.OpenSettings();
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
                        MessageBox.Show(ResourceManager.GetString("UnableToBackup"));
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
                        WindowsManager.OpenAbout();
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                    }
                });
            }
        }

        void CloseSavegame()
        {
            try
            {
                if (!IsSaveModified || MessageBox.Show(ResourceManager.GetString("SavegameWasModified"), ResourceManager.GetString("Warning"), MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
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
                        string path = FileManager.ExportVariables();
                        if (path == null)
                            return;
                        LeftInfoLine = ResourceManager.GetString("ExportingVariables");
                        Task.Run(() => ExportVariablesTask(path));
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                        MessageBox.Show(ResourceManager.GetString("UnableToExportVariables"));
                    }
                }, a => !IsSaveNull);
            }
        }

        #endregion


        public void UpdateVarCount()
        {
            var currentVarCount = DataGridVariables.Count;
            var totalVarCount = _openedSaveGame.HasValue ? _openedSaveGame.Value.VariablesList.Count : 0;
            RightInfoLine = $"{currentVarCount}/{totalVarCount}";
        }

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
                LeftInfoLine = ResourceManager.GetString("ImportingVariables");
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

                Dispatcher.Invoke(new Action(() =>
                {
                    DataGridVariables.Clear();
                    foreach (var p in _openedSaveGame.Value.VariablesList)
                        DataGridVariables.Add(p);
                }));
                SearchLine = "";
                if (changesList.Length != 0)
                {
                    MessageBox.Show(ResourceManager.GetString("VariablesWereImported") + "\n\n" + ResourceManager.GetString("ScriptActionList") + changesList);
                }
                else
                {
                    MessageBox.Show(ResourceManager.GetString("VariablesWereNotImported"));
                }
                if (notFoundVariables.Length > 0)
                {
                    MessageBox.Show(ResourceManager.GetString("NotFoundVariables") + notFoundVariables);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                MessageBox.Show(ResourceManager.GetString("UnableToImportVariables"));
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
                Dispatcher.Invoke(new Action(() =>
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
                        Dispatcher.Invoke(new Action(() =>
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
                MessageBox.Show(ResourceManager.GetString("UnableToLoadScripts"));
            }
        }

        private void SetDynamicInfo(string text, bool setToPath=false)
        {
            if (_dynamicInfo)
                return;
            _dynamicInfo = true;
            string backup = "";
            if (setToPath && !IsSaveNull)
            {
                backup = _openedSaveGame.Value.FilePath;
            }
            else
            {
                backup = LeftInfoLine;
            }
            LeftInfoLine = ResourceManager.GetString(text);
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
                List<GothicVariable> tempVariables = new SaveDatReader().Read(saveGamePath);
                if (tempVariables == null)
                    return;
                _openedSaveGame = new SaveGame(saveGamePath, tempVariables);

                Dispatcher.Invoke(() =>
                {
                    foreach (var p in _openedSaveGame.Value.VariablesList)
                        DataGridVariables.Add(p);
                });
                LeftInfoLine = _openedSaveGame.Value.FilePath;
                UpdateVarCount();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                SetDynamicInfo("UnableToLoadSavegame");
                MessageBox.Show(ResourceManager.GetString("SaveIsBrokenText") + "\n" +  ResourceManager.GetString(ex.Message));
            }
        }

        private void WriteVariablesToFile(string path = null)
        {
            if (path == null)
            {
                path = _openedSaveGame.Value.FilePath;
            }
            // No try here. It's handled before.
            Dispatcher.Invoke(() =>
            {
                var fstr = new FileStream(path, FileMode.Open, FileAccess.Write);
                var w = new BinaryWriter(fstr);
                foreach (var t in DataGridVariables.Where(t => !t.Modified))
                {
                    w.Seek(t.Position, 0);
                    w.Write(t.Value);
                    t.Saved();
                }
                w.Close();
                fstr.Close();
            });
        }


        private void Search()
        {
            if (IsSaveNull)
                return;
            try
            {
                var variables = _openedSaveGame.Value.VariablesList.Search(SearchLine);
                Dispatcher.Invoke(new Action(() =>
                {
                    DataGridVariables.Clear();
                    foreach (var p in variables)
                        DataGridVariables.Add(p);
                }));
                UpdateVarCount();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                MessageBox.Show(ResourceManager.GetString("UnableToSearch"));
            }
        }
    }
}