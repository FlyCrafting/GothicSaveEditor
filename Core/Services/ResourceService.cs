using System;
using System.Windows;

namespace GothicSaveEditor.Core.Services
{
    public static class ResourceServices
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
