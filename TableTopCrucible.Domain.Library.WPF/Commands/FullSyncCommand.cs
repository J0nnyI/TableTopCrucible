using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace TableTopCrucible.Domain.Library.WPF.Commands
{
    public class FullSyncCommand : ICommand
    {
        private readonly ILibraryManagementService libraryManagement;

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public FullSyncCommand(ILibraryManagementService libraryManagement)
        {
            this.libraryManagement = libraryManagement;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            this.libraryManagement.FullSync();
        }
    }
}
