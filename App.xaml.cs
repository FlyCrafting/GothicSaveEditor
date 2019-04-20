using System;
using System.Windows;

namespace GothicSaveEditor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        public static void SetLanguage(string lang = "")
        {
            ResourceDictionary dict = new ResourceDictionary();
            if (lang == "Русский" || lang == "Russian" || lang == "ru-RU")
            {
                dict.Source = new Uri("Resources/Russian.xaml", UriKind.Relative);
                Settings.Language = "Русский";
            }
            else
            {
                dict.Source = new Uri("Resources/English.xaml", UriKind.Relative);
                Settings.Language = "English";
            }
            Current.Resources.MergedDictionaries.Clear();
            Current.Resources.MergedDictionaries.Add(dict);
        }
    }
}
