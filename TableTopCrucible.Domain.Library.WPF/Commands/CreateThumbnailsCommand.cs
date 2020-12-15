using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Reactive.Linq;

using TableTopCrucible.Domain.Library.WPF.ViewModels;
using System.Linq;
using TableTopCrucible.Data.Models.Views;
using ReactiveUI;
using TableTopCrucible.Core.Models.Sources;

namespace TableTopCrucible.Domain.Library.WPF.Commands
{
    public class CreateThumbnailsCommand : ICommand
    {
        private readonly IThumbnailManagementService thumbnailManagementService;

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public CreateThumbnailsCommand(IThumbnailManagementService thumbnailManagementService)
        {
            this.thumbnailManagementService = thumbnailManagementService;
        }


        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter)
        {
            //ITaskProgressionInfo prog = null;
            switch (parameter)
            {
                case IEnumerable<ItemEx> items:
                    //prog=
                    thumbnailManagementService.CreateAndLinkThumbnail(items.ToArray());
                    break;
                case ItemEx item:
                    Observable.Start(
                        () => thumbnailManagementService.CreateAndLinkThumbnail(item),
                        RxApp.TaskpoolScheduler
                    );
                    break;
                case null:
                    throw new NotImplementedException($"{nameof(CreateThumbnailsCommand)}.{nameof(Execute)}: creating thumbnails for all items is not supported yet");
                    //prog = thumbnailManagementService.CreateAndLinkThumbnail();
                    //break;
                default:
                    throw new NotImplementedException($"{nameof(CreateThumbnailsCommand)}.{nameof(Execute)}: invalid parameter {parameter}");
            }
            //prog.ErrorChanges.TakeUntil(prog.DoneChanges).Where(ex=>ex != null).Subscribe(ex => MessageBox.Show(ex?.ToString(), "thumbnails could not be created"));

        }
    }
}
