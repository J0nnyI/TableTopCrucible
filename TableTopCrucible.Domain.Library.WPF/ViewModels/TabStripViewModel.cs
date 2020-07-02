using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using System.Windows.Navigation;

using TableTopCrucible.Core.Models.Sources;

namespace TableTopCrucible.Domain.Library.WPF.ViewModels
{
    public class TabStripViewModel : DisposableReactiveObjectBase
    {

        private readonly NavigationService _navigationService;

        public TabStripViewModel(NavigationService navigationService)
        {
            this._navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        }
    }
}
