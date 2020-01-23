using System.Windows;
using GothicSaveEditor.ViewModels;

namespace GothicSaveEditor
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            SettingsViewModel settingsViewModel = new SettingsViewModel(this);
            DataContext = settingsViewModel;
        }
    }
}
