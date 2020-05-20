using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace TableTopCrucible.WPF.ViewModels
{
    public class ValidatingViewModelBase:ViewModelBase
    {
        protected ObservableCollection<string> errorList = new ObservableCollection<string>();
        public ReadOnlyObservableCollection<string> ErrorList { get; }
        public ValidatingViewModelBase()
        {
            this.ErrorList = new ReadOnlyObservableCollection<string>(errorList);
        }
    }
}
