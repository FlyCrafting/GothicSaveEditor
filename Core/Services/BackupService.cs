﻿using System;
using System.IO;
using System.Linq;

namespace GothicSaveEditor.Core.Services
{
    public static class BackupService
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private static string BackupsFolder => Path.Combine(Directory.GetCurrentDirectory(), "Backups");

        public static void MakeBackup(string path)
        {
            if (!Directory.Exists(BackupsFolder))
            {
                Directory.CreateDirectory(BackupsFolder);
            }
            var date = DateTime.Now.ToString("yyyy-M-d_HH-mm-ss");
            var destPath = Path.Combine(BackupsFolder, date + ".SAV");
            File.Copy(path, destPath,true);
        }

        public static int BackupsCount => Directory.Exists(BackupsFolder) ? Directory.GetFiles(BackupsFolder).Count(f => f.EndsWith(".SAV")) : 0;

        public static Tuple<int, float> BackupsInfo
        {
            get
            {
                if (!Directory.Exists(BackupsFolder))
                {
                    return Tuple.Create(0, 0f);
                }

                var files = Directory.GetFiles(BackupsFolder);
                var count = 0;
                var totalSize = 0f;
                foreach (var file in files.Where(f => f.EndsWith(".SAV")))
                {
                    count++;
                    totalSize += new FileInfo(file).Length / 1024f / 1024; // in MegaBytes
                }

                return Tuple.Create(count, totalSize);
            }
        }


        public static bool DeleteAll()
        {
            if (!Directory.Exists(BackupsFolder))
                return true;

            var files = Directory.GetFiles(BackupsFolder);

            var sucess = true;
            foreach (var file in files.Where(f=> f.EndsWith(".SAV")))
            {
                try
                {
                    File.Delete(file);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                    sucess = false;
                }
            }

            return sucess;
        }
    }
}