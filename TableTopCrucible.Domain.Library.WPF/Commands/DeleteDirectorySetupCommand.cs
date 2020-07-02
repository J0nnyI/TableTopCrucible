using System;
using System.Windows.Input;

using TableTopCrucible.Data.Services;
using TableTopCrucible.Domain.Models.ValueTypes.IDs;

namespace TableTopCrucible.WPF.Commands
{
    public class DeleteDirectorySetupCommand : ICommand
    {
        private IDirectoryDataService _directorySetupService;
        public DeleteDirectorySetupCommand(IDirectoryDataService directorySetupService)
        {
            this._directorySetupService = directorySetupService;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
            => parameter is DirectorySetupId id && this._directorySetupService.CanDelete(id);
        public void Execute(object parameter)
        {
            if (parameter is DirectorySetupId id)
            {
                this._directorySetupService.Delete(id);
            }
        }
    }
}
