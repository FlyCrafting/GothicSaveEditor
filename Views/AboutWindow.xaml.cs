using GothicSaveEditor.ViewModels;
using System.Windows;
using System.Windows.Navigation;

namespace GothicSaveEditor.Views
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
            AboutViewModel aboutViewModel = new AboutViewModel(this);
            DataContext = aboutViewModel;
        }

        public void GoToSite(object sender, RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Uri.ToString());
        }
    }
}
