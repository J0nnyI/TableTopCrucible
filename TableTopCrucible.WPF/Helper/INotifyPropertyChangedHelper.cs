using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace TableTopCrucible.WPF.Helper
{
    public static class INotifyPropertyChangedHelper
    {
        public static void OnPropertyChange<T>(this INotifyPropertyChanged src, PropertyChangedEventHandler propertyChange, T value, ref T field, [CallerMemberName] string propName = "")
        {
            if (field?.Equals(value) != true)
            {
                field = value;
                propertyChange?.Invoke(src, new PropertyChangedEventArgs(propName));
            }
        }
    }
}
