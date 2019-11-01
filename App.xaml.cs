using System;
using System.Linq;
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
            var dict = new ResourceDictionary();
            switch (lang)
            {
                case "Russian":
                    dict.Source = new Uri("Resources/Russian.xaml", UriKind.Relative);
                    Settings.Language = "Russian";
                    break;
                default:
                    dict.Source = new Uri("Resources/English.xaml", UriKind.Relative);
                    Settings.Language = "English";
                    break;
            }
            var oldDict = (from d in Current.Resources.MergedDictionaries
                where d.Source != null && d.Source.OriginalString.StartsWith("Resources")
                select d).First();
            if (oldDict != null)
            {
                var ind = Current.Resources.MergedDictionaries.IndexOf(oldDict);
                Current.Resources.MergedDictionaries.Remove(oldDict);
                Current.Resources.MergedDictionaries.Insert(ind, dict);
            }
            else
            {
                Current.Resources.MergedDictionaries.Add(dict);
            }
        }
    }
}
