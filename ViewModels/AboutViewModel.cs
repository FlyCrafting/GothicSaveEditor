using GothicSaveEditor.Views;
using System.ComponentModel;


namespace GothicSaveEditor.ViewModels
{
    public class AboutViewModel:INotifyPropertyChanged
    {
        #region PropertyChanged (for binding)
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
        #endregion

        AboutWindow aboutWindow;

        public AboutViewModel(AboutWindow aboutWindow)
        {
            this.aboutWindow = aboutWindow;
        }

        public RelayCommand CloseWindowCommand
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    aboutWindow.Close();
                });
            }
        }
    }
}
