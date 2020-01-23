using FolderBrowserDialog =  System.Windows.Forms;
using Microsoft.Win32;
using System.IO;
using System;
using GothicSaveEditor.Core.Utils;

namespace GothicSaveEditor.Core.Services
{
    public static class FileService
    {
        private const string FileName = "SAVEDAT"; //FileName
        private const string DefaultExt = "SAVEDAT"; //File Extensinon
        private const string Filter = "Gothic Savegames|SAVEDAT.SAV"; //Filter by gothic savegames

        public static string PickGothicFolder()
        {
            var openFolderDialog = new FolderBrowserDialog.FolderBrowserDialog();

            if (openFolderDialog.ShowDialog() == FolderBrowserDialog.DialogResult.OK)
            {
                return openFolderDialog.SelectedPath;
            }
            return null;
        }

        public static string ImportSave()
        {
            var openFileDialog = new OpenFileDialog
            {
                InitialDirectory = Settings.GamePath,
                FileName = FileName,
                DefaultExt = DefaultExt,
                Filter = Filter,
                RestoreDirectory = true
            };
            return openFileDialog.ShowDialog() == true ? openFileDialog.FileName : null;
        }

        public static string ExportSave()
        {
            var saveFileDialog = new SaveFileDialog
            {
                InitialDirectory = Settings.GamePath,
                FileName = FileName,
                DefaultExt = DefaultExt,
                Filter = Filter,
                RestoreDirectory = true
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                return saveFileDialog.FileName;
            }
            return null;
        }

        public static string ExportVariables()
        {
            var saveFileDialog = new SaveFileDialog
            {
                InitialDirectory = Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory),
                DefaultExt = "txt",
                Filter = "Text Files|*.txt",
                RestoreDirectory = false
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                return saveFileDialog.FileName;
            }
            return null;
        }
    }
}
