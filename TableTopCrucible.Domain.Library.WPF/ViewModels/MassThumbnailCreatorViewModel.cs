using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using TableTopCrucible.Data.Services;
using TableTopCrucible.WPF.Commands;

namespace TableTopCrucible.Domain.Library.WPF.ViewModels
{
    public class MassThumbnailCreatorViewModel
    {
        private readonly IItemService itemService;
        public ICommand StartProcess { get; }

        [Reactive] Model3D ViewportContent { get; set; }

        public MassThumbnailCreatorViewModel(
            IItemService itemService
            )
        {
            this.itemService = itemService;
            this.StartProcess = new RelayCommand(_=>_startProcess());
        }
        private void _startProcess()
        {

        }
    }
}
