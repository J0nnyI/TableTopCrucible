using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Text;
using System.Windows.Input;

using TableTopCrucible.Domain.Models.Sources;
using TableTopCrucible.Domain.Services;

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
        private IDirectorySetupService _directorySetupService;
        public SaveDirectorySetupCommand(IDirectorySetupService directorySetupService)
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
