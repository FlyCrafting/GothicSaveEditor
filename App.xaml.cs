using System;
using System.Linq;
using System.Windows;
using GothicSaveEditor.Core.Utils;

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
                case "Русский":
                    dict.Source = new Uri("Resources/Russian.xaml", UriKind.Relative);
                    Options.Language = "Русский";
                    break;
                default:
                    dict.Source = new Uri("Resources/English.xaml", UriKind.Relative);
                    Options.Language = "English";
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
