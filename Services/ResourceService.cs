using System.Windows;

namespace GothicSaveEditor.Services
{
    public static class ResourceService
    {
        public static string GetString(string resourceName)
        {
            return Application.Current.FindResource(resourceName).ToString();
        }
    }
}
