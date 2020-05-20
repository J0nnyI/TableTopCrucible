using System.ComponentModel;
using System.Runtime.CompilerServices;

using TableTopCrucible.WPF.Helper;

namespace TableTopCrucible.WPF.ViewModels
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void onPropertyChange<T>(T value, ref T field, [CallerMemberName] string propName = "")
        {
            ((INotifyPropertyChanged)this).OnPropertyChange(PropertyChanged, value,ref field, propName);
        }
        protected void raisePropertyChanged(string propName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}
