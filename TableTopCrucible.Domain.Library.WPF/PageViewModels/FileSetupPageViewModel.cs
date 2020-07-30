using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Text;

using TableTopCrucible.Core.WPF.PageViewModels;
using TableTopCrucible.Domain.Library.WPF.ViewModels;
using TableTopCrucible.Domain.Library.WPF.Commands;
using System.Windows.Input;

namespace TableTopCrucible.Domain.Library.WPF.PageViewModels
{
    public class FileSetupPageViewModel : PageViewModelBase
    {
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
            SaveFileDialogCommand saveFile            
            ) : base("File Setup", PackIconKind.File)
        {
            this.DirList = dirList;
            this.FileList = fileList;
            this.ItemList = itemList;
            this.NotificationCenter = notificationCenter;
            OpenFile = openFile;
            SaveFile = saveFile;
        }

    }
}
