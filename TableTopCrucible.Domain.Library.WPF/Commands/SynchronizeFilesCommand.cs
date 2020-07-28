using ReactiveUI;
using System;
using System.Reactive.Linq;
using System.Windows.Input;

using TableTopCrucible.Data.Services;
using TableTopCrucible.Domain.Library;

namespace TableTopCrucible.WPF.Commands
{

    public class SynchronizeFilesCommand : ICommand
    {
        private readonly ILibraryManagementService libraryManagement;

        public SynchronizeFilesCommand(ILibraryManagementService libraryManagement)
        {
            this.libraryManagement = libraryManagement;
        }

        public event EventHandler CanExecuteChanged;
        public event EventHandler<ItemCreatedEventArgs> ItemCreated;

        public bool CanExecute(object parameter) => true;
        public void Execute(object parameter)
        {
            Observable.Start(() =>
            {
                this.libraryManagement.SynchronizeFiles();
            }, RxApp.TaskpoolScheduler);
        }
    }
}