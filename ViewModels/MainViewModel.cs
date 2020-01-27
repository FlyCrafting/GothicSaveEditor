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
using System.Windows.Input;
using GothicSaveEditor.Core.Primitives;
using GothicSaveEditor.Core.Readers;
using GothicSaveEditor.Core.Services;
using GothicSaveEditor.Core.Utils;
using NLog;

namespace GothicSaveEditor.ViewModels
{
    public partial class MainViewModel : INotifyPropertyChanged
    {
        #region PropertyChanged (for binding)
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
        #endregion
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        
        public ObservableCollection<GothicVariable> DataGridVariables { get; set; } = new ObservableCollection<GothicVariable>();
        public ObservableCollection<Script> Scripts { get; set; } = new ObservableCollection<Script>();

        private SaveGame? _openedSaveGame;

        private bool _dynamicInfo;

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
                    if (DataGridVariables.Any(t => t.Modified))
                    {
                        isChanged = true;
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
                Search();
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
        
        public string GseVersion => Options.GseVersion;


        bool CanCloseSave =>
            (!IsSaveModified
             || MessageBox.Show(ResourceServices.GetString("SavegameWasModified"),
                 ResourceServices.GetString("Warning"), MessageBoxButton.YesNo, MessageBoxImage.Warning) ==
             MessageBoxResult.Yes);

        #region HelpMethods
        void ClearWorkSpace()
        {
            try
            {
                DataGridVariables.Clear();
                Scripts.Clear();
                _openedSaveGame = null;
                LeftInfoLine = "";
                RightInfoLine = "";
                SearchLine = "";
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }


        public void UpdateVarCount()
        {
            var currentVarCount = DataGridVariables.Count;
            var totalVarCount = _openedSaveGame.HasValue ? _openedSaveGame.Value.VariablesList.Count : 0;
            RightInfoLine = $"{currentVarCount}/{totalVarCount}";
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
            LeftInfoLine = ResourceServices.GetString(text);
            Task.Delay(Options.InfoLinePopUpTime).ContinueWith(t => ResetDynamicInfo(backup));
        }

        private void ResetDynamicInfo(string toText)
        {
            LeftInfoLine = toText;
            _dynamicInfo = false;
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
                LeftInfoLine = ResourceServices.GetString("ImportingVariables");
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
                    MessageBox.Show(ResourceServices.GetString("VariablesWereImported") + "\n\n" + ResourceServices.GetString("ScriptActionList") + changesList);
                }
                else
                {
                    MessageBox.Show(ResourceServices.GetString("VariablesWereNotImported"));
                }
                if (notFoundVariables.Length > 0)
                {
                    MessageBox.Show(ResourceServices.GetString("NotFoundVariables") + notFoundVariables);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                MessageBox.Show(ResourceServices.GetString("UnableToImportVariables"));
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
                if (!Directory.Exists(Environment.CurrentDirectory + Options.ScriptsDirectory))
                {
                    Directory.CreateDirectory(Environment.CurrentDirectory + Options.ScriptsDirectory);
                    return;
                }
                string[] dirs = Directory.GetFiles(Environment.CurrentDirectory + Options.ScriptsDirectory, "*.gses");
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
                MessageBox.Show(ResourceServices.GetString("UnableToLoadScripts"));
            }
        }

        private void LoadSaveGame(string saveGamePath)
        {
            //Path is always not null here!
            try
            {
                var tempVariables = SaveDatReader.Read(saveGamePath);
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
                MessageBox.Show(ResourceServices.GetString("SaveIsBrokenText") + "\n" +  ResourceServices.GetString(ex.Message));
            }
        }

        private void WriteVariablesToFile(string path)
        {
            if (IsSaveNull)
                return;
            
            // No try here. It's handled before.
            Dispatcher.Invoke(() =>
            {
                var fstr = new FileStream(path, FileMode.Open, FileAccess.Write);
                var w = new BinaryWriter(fstr);
                foreach (var t in DataGridVariables.Where(t => t.Modified))
                {
                    w.Seek(t.Position, 0);
                    w.Write(t.Value);
                    t.SetUnModified();
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
                Dispatcher.Invoke(() =>
                {
                    DataGridVariables.Clear();
                    foreach (var p in variables)
                        DataGridVariables.Add(p);
                });
                UpdateVarCount();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                MessageBox.Show(ResourceServices.GetString("UnableToSearch"));
            }
        }
    }
}