using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Input;

using TableTopCrucible.Domain.Library.WPF.ViewModels;

namespace TableTopCrucible.Domain.Library.WPF.Commands
{
    public class CreateAllThumbnailsCommand : ICommand
    {
        public ILibraryManagementService LibraryManagementService { get; }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public CreateAllThumbnailsCommand(ILibraryManagementService libraryManagementService)
        {
            LibraryManagementService = libraryManagementService;
        }


        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter)
        {
            LibraryManagementService.GenerateAllThumbnails();
        }
    }
}
