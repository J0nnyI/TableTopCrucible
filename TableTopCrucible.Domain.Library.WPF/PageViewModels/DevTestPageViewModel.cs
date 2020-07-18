using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using TableTopCrucible.Core.WPF.PageViewModels;
using TableTopCrucible.Domain.Library.WPF.Commands;
using TableTopCrucible.Domain.Library.WPF.ViewModels;
using TableTopCrucible.WPF.Commands;

namespace TableTopCrucible.Domain.Library.WPF.PageViewModels
{
    public class DevTestPageViewModel : PageViewModelBase
    {

        public DevTestPageViewModel(
            SaveFileDialogCommand saveFile,
            OpenFileDialogCommand openFile,
            DirectoryListViewModel dirList,
            NotificationCenterViewModel notificationCenter
            ) : base("dev test", PackIconKind.DeveloperBoard)
        {
            this.SaveFile = saveFile;
            this.OpenFile = openFile;
            this.DirList = dirList;
            this.NotificationCenter = notificationCenter;
        }

        public ICommand SaveFile { get; }
        public ICommand OpenFile { get; }
        public DirectoryListViewModel DirList { get; }
        public NotificationCenterViewModel NotificationCenter { get; }
    }
}
