using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TableTopCrucible.WPF.ViewModels
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void onPropertyChange<T>(T value, ref T field, [CallerMemberName] string propName = "")
        {
            if (field?.Equals(value) != true)
            {
                field = value;
                this.raisePropertyChanged(propName);
            }
        }
        protected void raisePropertyChanged(string propName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}
