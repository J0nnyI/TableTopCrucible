using ReactiveUI;

using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Input;

using TableTopCrucible.Core.Models.Sources;
using TableTopCrucible.Data.Models.Sources;
using TableTopCrucible.Data.Services;
using TableTopCrucible.Domain.Models.ValueTypes.IDs;

namespace TableTopCrucible.Domain.Library.WPF.Commands
{
    public class RemoveDirectorySetupRecursivelyCommand : DisposableReactiveObjectBase, ICommand
    {
        private readonly ILibraryManagementService libraryManagementService;
        private readonly IDirectoryDataService directoryDataService;

        public RemoveDirectorySetupRecursivelyCommand(ILibraryManagementService libraryManagementService, IDirectoryDataService directoryDataService)
        {
            this.libraryManagementService = libraryManagementService;
            this.directoryDataService = directoryDataService;
            directoryDataService
                .Get()
                .Connect()
                .TakeUntil(Destroy)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ =>
                {
                    this.CanExecuteChanged?.Invoke(this, new EventArgs());
                });
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
            //switch (parameter)
            //{
            //    case DirectorySetup dirSetup:
            //        return directoryDataService.CanDelete(dirSetup.Id);
            //    case DirectorySetupId dirSetupId:
            //        return directoryDataService.CanDelete(dirSetupId);
            //}
            //return false;
        }

        public void Execute(object parameter)
        {
            if (parameter is DirectorySetup setup)
                this.libraryManagementService.RemoveDirectorySetupRecursively(setup.Id);
            else if (parameter is DirectorySetupId id)
                this.libraryManagementService.RemoveDirectorySetupRecursively(id);
            else
                throw new InvalidOperationException($"{nameof(RemoveDirectorySetupRecursivelyCommand)}: invalid parameter type: {parameter.GetType()} ({parameter})");

        }
    }
}
