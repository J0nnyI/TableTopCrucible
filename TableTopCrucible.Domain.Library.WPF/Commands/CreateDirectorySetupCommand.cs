using System;
using System.Windows.Input;

using TableTopCrucible.Data.Services;
using TableTopCrucible.Data.Models.Sources;

namespace TableTopCrucible.WPF.Commands
{
    public class CreateDirectorySetupCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        private IDirectoryDataService _directorySetupService;
        public CreateDirectorySetupCommand(IDirectoryDataService directorySetupService)
        {
            this._directorySetupService = directorySetupService;
        }

        public bool CanExecute(object parameter) => true;
        public void Execute(object parameter)
        {
            this._directorySetupService.Patch(new DirectorySetupChangeset()
            {
                Path = @"C:\",
                Name = "New Directory",
                Description = "a new directory which has not been setup yet"
            });
        }
    }
}
