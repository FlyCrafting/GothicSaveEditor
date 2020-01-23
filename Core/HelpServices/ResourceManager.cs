using System;
using System.Windows;

namespace GothicSaveEditor.Core.HelpServices
{
    public static class ResourceManager
    {
        public static string GetString(string resourceName)
        {
            try
            {
                var res = Application.Current.FindResource(resourceName);
                return res == null ? "" : res.ToString();
            }
            catch
            {
                return "";
            }
        }
    }
}
