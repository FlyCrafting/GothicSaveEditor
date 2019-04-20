using GothicSaveEditor.ViewModel;

namespace GothicSaveEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            //App entry point
            InitializeComponent();

            //Creating View Model and set its DataContext for binding
            MainViewModel viewModelLogic = new MainViewModel();
            DataContext = viewModelLogic;
        }
    }
}
