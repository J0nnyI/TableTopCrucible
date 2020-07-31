using DynamicData;
using MaterialDesignThemes.Wpf;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Text;
using System.Windows.Input;
using TableTopCrucible.Core.WPF.PageViewModels;
using TableTopCrucible.Data.Models.Views;
using TableTopCrucible.Data.Services;
using TableTopCrucible.Domain.Library.WPF.Commands;
using TableTopCrucible.Domain.Library.WPF.ViewModels;
using TableTopCrucible.Domain.Models.Sources;
using TableTopCrucible.WPF.Commands;

namespace TableTopCrucible.Domain.Library.WPF.PageViewModels
{
    public class DevTestPageViewModel : PageViewModelBase
    {
        public ReadOnlyObservableCollection<VersionedFile> _vFiles;
        public ReadOnlyObservableCollection<VersionedFile> Vfiles => _vFiles;

        public ReadOnlyObservableCollection<ExtendedItem> _items;
        public ReadOnlyObservableCollection<ExtendedItem> Items => _items;

        public DevTestPageViewModel(
            SaveFileDialogCommand saveFile,
            OpenFileDialogCommand openFile,
            IItemService itemService,
            DirectoryListViewModel dirList,
            NotificationCenterViewModel notificationCenter,
            IFileItemLinkService fileItemLink
            ) : base("dev test", PackIconKind.DeveloperBoard)
        {
            this.SaveFile = saveFile;
            this.OpenFile = openFile;
            this.DirList = dirList;
            this.NotificationCenter = notificationCenter;



            fileItemLink.GetVersionedFilesByHash()
                .Connect()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _vFiles)
                .Subscribe();


            itemService.GetExtended()
                .Connect()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _items)
                .Subscribe();
        }

        public ICommand SaveFile { get; }
        public ICommand OpenFile { get; }
        public DirectoryListViewModel DirList { get; }
        public NotificationCenterViewModel NotificationCenter { get; }
    }
}
