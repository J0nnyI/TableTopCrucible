using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Text;

using TableTopCrucible.Core.WPF.PageViewModels;
using TableTopCrucible.Domain.Library.WPF.ViewModels;

namespace TableTopCrucible.Domain.Library.WPF.PageViewModels
{
    public class DevTestPageViewModel : PageViewModelBase
    {
        public DirectoryListViewModel DirList { get; }
        public FileListViewModel FileList { get; }
        public ItemListViewModel ItemList { get; }
        public NotificationCenterViewModel NotificationCenter { get; }

        public DevTestPageViewModel(
            DirectoryListViewModel dirList,
            FileListViewModel fileList, 
            ItemListViewModel itemList,
            NotificationCenterViewModel notificationCenter
            ) : base("dev test", PackIconKind.DeveloperBoard)
        {
            this.DirList = dirList;
            this.FileList = fileList;
            this.ItemList = itemList;
            this.NotificationCenter = notificationCenter;
        }

    }
}
