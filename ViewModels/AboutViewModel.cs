using GothicSaveEditor.Views;
using System.ComponentModel;
using GothicSaveEditor.Core.Primitives;


namespace GothicSaveEditor.ViewModels
{
    public class AboutViewModel: INotifyPropertyChanged
    {
        #region PropertyChanged (for binding)
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
        #endregion

        private readonly AboutWindow _aboutWindow;

        public AboutViewModel(AboutWindow aboutWindow)
        {
            _aboutWindow = aboutWindow;
        }

        public RelayCommand CloseWindowCommand
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    _aboutWindow.Close();
                });
            }
        }
    }
}
