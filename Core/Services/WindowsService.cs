using System.Linq;
using System.Windows;

namespace GothicSaveEditor.Core.Services
{
    public static class WindowsService
    {
        public static bool IsWindowOpen<T>(string name = "") where T : Window
        {
            return string.IsNullOrEmpty(name)
                ? Application.Current.Windows.OfType<T>().Any()
                : Application.Current.Windows.OfType<T>().Any(w => w.Name.Equals(name));
        }

        public static void OpenSettings()
        {
            if (IsWindowOpen<SettingsWindow>())
                return;
            var sw = new SettingsWindow();
            sw.ShowDialog();
        }
    }
}
