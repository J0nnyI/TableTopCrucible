using Microsoft.Win32;

using ReactiveUI;

using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Linq;
using System.Text;
using System.Windows.Input;

using TableTopCrucible.Core.Services;

namespace TableTopCrucible.Domain.Library.WPF.Commands
{
    public class OpenFileDialogCommand : ICommand
    {
        private readonly ISaveService _saveService;

        public event EventHandler CanExecuteChanged;
        public OpenFileDialogCommand( ISaveService saveService)
        {
            this._saveService = saveService;
        }
        public bool CanExecute(object parameter) => true;
        public void Execute(object parameter)
        {
            Observable.Start(()=>
            {
                var dialog = new OpenFileDialog();
                dialog.Filter = "Table Top Crucible Library (*.ttcl)|*.*";
                if (dialog.ShowDialog() == true)
                    _saveService.Load(dialog.FileName);
            }, RxApp.TaskpoolScheduler);

        }
    }
}
