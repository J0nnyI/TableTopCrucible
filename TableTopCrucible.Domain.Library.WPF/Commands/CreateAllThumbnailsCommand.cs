using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Reactive.Linq;

using TableTopCrucible.Domain.Library.WPF.ViewModels;
using System.Linq;

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
            LibraryManagementService.GenerateAllThumbnails()
                .Select(prog=>
                    prog.TaskProgressionStateChanges
                        .Where(prog=>prog.IsDone)
                ).CombineLatest()
                .Subscribe(res=> {
                    var msg = string.Join(Environment.NewLine,
                        res.Where(x => x.Error != null).Select(x => x.Error.Message)
                        );
                    MessageBox.Show(msg, "failed");
                });
        }
    }
}
