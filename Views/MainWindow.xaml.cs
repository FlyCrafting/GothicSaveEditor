using System;
using GothicSaveEditor.Core.Services;
using GothicSaveEditor.ViewModels;

namespace GothicSaveEditor.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            
            var viewModelLogic = new MainViewModel();
            DataContext = viewModelLogic;
            
            // Open with program handling
            var args = Environment.GetCommandLineArgs();
            if (args.Length >= 2)
            {
                var path = args[1];
                if (path.EndsWith("SAVEDAT.SAV"))
                {
                    viewModelLogic.LoadSaveGame(path);
                }
            }
            
            AssociationService.EnsureAssociationsSet();
        }
    }
}
