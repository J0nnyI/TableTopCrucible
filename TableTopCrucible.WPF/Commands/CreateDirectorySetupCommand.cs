using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using TableTopCrucible.Domain.Models.Sources;
using TableTopCrucible.Domain.Services;

namespace TableTopCrucible.WPF.Commands
{
    public class CreateDirectorySetupCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        private IDirectorySetupService _directorySetupService;
        public CreateDirectorySetupCommand(IDirectorySetupService directorySetupService)
        {
            this._directorySetupService = directorySetupService;
        }

        public bool CanExecute(object parameter) => true;
        public void Execute(object parameter)
        {
            this._directorySetupService.Patch(new DirectorySetupChangeset()
            {
                Path = null,
                Name = "New Directory",
                Description = "a new directory which has not been setup yet"
            });
        }
    }
}
