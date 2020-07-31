using System;
using System.Windows.Input;

using TableTopCrucible.Data.Services;
using TableTopCrucible.Domain.Library;

namespace TableTopCrucible.WPF.Commands
{
    public class GenerateItemsFromFilesCommand : ICommand
    {
        private readonly ILibraryManagementService libraryManagement;

        public GenerateItemsFromFilesCommand(ILibraryManagementService libraryManagement)
        {
            this.libraryManagement = libraryManagement;
        }


        //todo^: notify changes when there are files withour item
        public event EventHandler CanExecuteChanged;

        //todo
        public bool CanExecute(object parameter) => true;
        public void Execute(object parameter)
        {
            this.libraryManagement.AutoGenerateItems();
        }
    }
}
