using System;
using System.Windows.Input;

using TableTopCrucible.Data.Services;
using TableTopCrucible.Domain.Models.Sources;

namespace TableTopCrucible.WPF.Commands
{

    public struct DirectorySetupSavedEventArgs
    {
        public DirectorySetupSavedEventArgs(DirectorySetup directorySetup)
        {
            this.DirectorySetup = directorySetup;
        }

        public DirectorySetup DirectorySetup { get; }
    }
    public class SaveDirectorySetupCommand : ICommand
    {
        private IDirectoryDataService _directorySetupService;
        public SaveDirectorySetupCommand(IDirectoryDataService directorySetupService)
        {
            this._directorySetupService = directorySetupService;
        }

        public event EventHandler<DirectorySetupSavedEventArgs> DirectorySetupSaved;
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter) => parameter is DirectorySetupChangeset;
        public void Execute(object parameter)
        {
            if (parameter is DirectorySetupChangeset changeset)
            {
                var entity = this._directorySetupService.Patch(changeset);
                DirectorySetupSaved?.Invoke(this, new DirectorySetupSavedEventArgs(entity));
            }
        }
    }
}
