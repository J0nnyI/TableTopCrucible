using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Text;

using TableTopCrucible.Core.WPF.PageViewModels;
using TableTopCrucible.Domain.Library.WPF.ViewModels;
using TableTopCrucible.Domain.Library.WPF.Commands;
using System.Windows.Input;
using TableTopCrucible.Core.Services;
using TableTopCrucible.Core.WPF.Commands;
using TableTopCrucible.Core.WPF.ViewModels;
using ReactiveUI;
using System.Reactive.Linq;
using TableTopCrucible.Data.Files.Scanner;

namespace TableTopCrucible.Domain.Library.WPF.PageViewModels
{
    public class FileSetupPageViewModel : PageViewModelBase
    {
        private readonly IFileManagementService _fileManagementService;

        public DirectoryListViewModel DirList { get; }
        public NotificationCenterViewModel NotificationCenter { get; }
        public ICommand OpenFile { get; }
        public ICommand SaveFile { get; }
        public ITaskProgressBar TaskProgressBar { get; }

        public FileSetupPageViewModel(
            DirectoryListViewModel dirList,
            NotificationCenterViewModel notificationCenter,
            OpenFileDialogCommand openFile,
            SaveFileDialogCommand saveFile,
            ITaskProgressBar taskProgressBar,
            IFileManagementService fileManagementService,
            ISaveService saveService
            ) : base("File Setup", PackIconKind.File)
        {
            this.DirList = dirList;
            this.NotificationCenter = notificationCenter;
            OpenFile = openFile;
            SaveFile = saveFile;
            TaskProgressBar = taskProgressBar;
            _fileManagementService = fileManagementService;
            this.SaveFileAction = (path) => saveService.Save(path);
            this.WhenAnyValue(vm => vm._fileManagementService.TotalProgress)
                .ObserveOn(RxApp.MainThreadScheduler)
                .SubscribeOn(RxApp.MainThreadScheduler)
                .BindTo(this, vm => vm.TaskProgressBar.PrimaryProgress);
            this.WhenAnyValue(vm => vm._fileManagementService.SubProgress)
                .ObserveOn(RxApp.MainThreadScheduler)
                .SubscribeOn(RxApp.MainThreadScheduler)
                .BindTo(this, vm => vm.TaskProgressBar.SecondaryProgress);
        }

        public Action<string> SaveFileAction { get; }
    }
}
