using System;
using System.Windows;

namespace GothicSaveEditor.Core.HelpServices
{
    public static class Dispatcher
    {
        public static void Invoke(Action action)
        {
            System.Windows.Threading.Dispatcher dispatchObject = Application.Current.Dispatcher;
            if (dispatchObject == null || dispatchObject.CheckAccess())
            {
                action();
            }
            else
            {
                dispatchObject.Invoke(action);
            }
        }
    }
}
