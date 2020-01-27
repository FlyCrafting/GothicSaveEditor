using System;
using System.IO;

namespace GothicSaveEditor.Core.Services
{
    public static class BackupService
    {
        private const string BackupsFolder = "Backups";

        public static void MakeBackup(string path)
        {
            if (!Directory.Exists(BackupsFolder))
            {
                Directory.CreateDirectory(BackupsFolder);
            }
            var date = DateTime.Now.ToString("yyyy-M-d_HH-mm-ss");
            var destPath = Path.Combine(Directory.GetCurrentDirectory(), BackupsFolder, date + ".SAV");
            File.Copy(path, destPath,true);
        }
    }
}