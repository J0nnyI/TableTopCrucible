using ReactiveUI;

using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

using TableTopCrucible.Core.Models.Sources;
using TableTopCrucible.Data.Files.Scanner;

namespace TableTopCrucible.Domain.Library.WPF.Commands
{
    public class FullSyncCommand : DisposableReactiveObjectBase, ICommand
    {
        private readonly IFileManagementService _fileManagementService;

        public event EventHandler CanExecuteChanged;

        public FullSyncCommand(IFileManagementService fileManagementService)
        {
            _fileManagementService = fileManagementService;
            _fileManagementService
                .IsSynchronizingChanges
                .TakeUntil(Destroy)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(canSync => this.CanExecuteChanged?.Invoke(this, new EventArgs()));
        }

        public bool CanExecute(object parameter)
        {
            return !_fileManagementService.IsSynchronizing;
        }

        public void Execute(object parameter)
        {
            try
            {
                this._fileManagementService.StartSynchronization();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "File Synchronization failed");
            }
        }
    }
}
