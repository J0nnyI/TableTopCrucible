using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Text;

using TableTopCrucible.Core.WPF.PageViewModels;
using TableTopCrucible.Domain.Library.WPF.ViewModels;
using TableTopCrucible.Domain.Library.WPF.Commands;
using System.Windows.Input;
using TableTopCrucible.Core.Services;

namespace TableTopCrucible.Domain.Library.WPF.PageViewModels
{
    public class FileSetupPageViewModel : PageViewModelBase
    {
        private readonly ISaveService saveService;

        public DirectoryListViewModel DirList { get; }
        public FileListViewModel FileList { get; }
        public ItemListViewModel ItemList { get; }
        public NotificationCenterViewModel NotificationCenter { get; }
        public ICommand OpenFile { get; }
        public ICommand SaveFile { get; }

        public FileSetupPageViewModel(
            DirectoryListViewModel dirList,
            FileListViewModel fileList,
            ItemListViewModel itemList,
            NotificationCenterViewModel notificationCenter,
            OpenFileDialogCommand openFile,
            SaveFileDialogCommand saveFile,
            ISaveService saveService
            ) : base("File Setup", PackIconKind.File)
        {
            this.DirList = dirList;
            this.FileList = fileList;
            this.ItemList = itemList;
            this.NotificationCenter = notificationCenter;
            OpenFile = openFile;
            SaveFile = saveFile;
            this.saveService = saveService;
        }

        public Action<string> OpenFileAction => saveService.Load;
        public Action<string> SaveFileAction => saveService.Save;
    }
}
