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

namespace TableTopCrucible.Domain.Library.WPF.PageViewModels
{
    public class FileSetupPageViewModel : PageViewModelBase
    {
        private readonly ISaveService saveService;

        public DirectoryListViewModel DirList { get; }
        public NotificationCenterViewModel NotificationCenter { get; }
        public ICommand OpenFile { get; }
        public ICommand SaveFile { get; }

        public FileSetupPageViewModel(
            DirectoryListViewModel dirList,
            NotificationCenterViewModel notificationCenter,
            OpenFileDialogCommand openFile,
            SaveFileDialogCommand saveFile,
            ISaveService saveService
            ) : base("File Setup", PackIconKind.File)
        {
            this.DirList = dirList;
            this.NotificationCenter = notificationCenter;
            OpenFile = openFile;
            SaveFile = saveFile;
            this.saveService = saveService;
            this.SaveFileAction = (path) => saveService.Save(path);
        }

        public Action<string> OpenFileAction => throw new NotImplementedException("moved to another location");
        public Action<string> SaveFileAction { get; }
    }
}
