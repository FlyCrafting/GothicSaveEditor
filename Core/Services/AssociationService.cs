using System;
using System.Diagnostics;
using Microsoft.Win32;

namespace GothicSaveEditor.Core.Services
{
    public class FileAssociation
    {
        public string Extension { get; set; }
        public string ProgId { get; set; }
        public string FileTypeDescription { get; set; }
        public string ExecutableFilePath { get; set; }
    }

    public static class AssociationService
    {
        // needed so that Explorer windows get refreshed after the registry is updated
        [System.Runtime.InteropServices.DllImport("Shell32.dll")]
        private static extern int SHChangeNotify(int eventId, int flags, IntPtr item1, IntPtr item2);

        private const int ShcneAssocchanged = 0x8000000;
        private const int ShcnfFlush = 0x1000;

        public static void EnsureAssociationsSet()
        {
            var processModule = Process.GetCurrentProcess().MainModule;
            if (processModule != null)
            {
                var filePath = processModule.FileName;
                
                EnsureAssociationsSet(new FileAssociation
                    {
                        Extension = ".SAV",
                        ProgId = "Gothic_Save_Editor",
                        FileTypeDescription = "Savegame File",
                        ExecutableFilePath = filePath
                    });
            }
        }

        private static void EnsureAssociationsSet(FileAssociation association)
        {
            bool madeChanges = false;
            madeChanges |= SetAssociation(
                association.Extension,
                association.ProgId,
                association.FileTypeDescription,
                association.ExecutableFilePath);

            if (madeChanges)
            {
                SHChangeNotify(ShcneAssocchanged, ShcnfFlush, IntPtr.Zero, IntPtr.Zero);
            }
        }

        private static bool SetAssociation(string extension, string progId, string fileTypeDescription, string applicationFilePath)
        {
            bool madeChanges = false;
            madeChanges |= SetKeyDefaultValue(@"Software\Classes\" + extension, progId);
            madeChanges |= SetKeyDefaultValue(@"Software\Classes\" + progId, fileTypeDescription);
            madeChanges |= SetKeyDefaultValue($@"Software\Classes\{progId}\shell\open\command", "\"" + applicationFilePath + "\" \"%1\"");
            return madeChanges;
        }

        private static bool SetKeyDefaultValue(string keyPath, string value)
        {
            using (var key = Registry.CurrentUser.CreateSubKey(keyPath))
            {
                if (key.GetValue(null) as string != value)
                {
                    key.SetValue(null, value);
                    return true;
                }
            }

            return false;
        }
    }
}