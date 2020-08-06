using DynamicData;

using Microsoft.Win32.SafeHandles;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using TableTopCrucible.Core.Models.Sources;
using TableTopCrucible.Core.Services;
using TableTopCrucible.Core.WPF.Helper;
using TableTopCrucible.Data.Models.Views;
using TableTopCrucible.Data.Services;
using TableTopCrucible.Domain.Models.Sources;
using TableTopCrucible.Domain.Models.ValueTypes;
using TableTopCrucible.Domain.Models.ValueTypes.IDs;
using TableTopCrucible.WPF.Commands;

using FileInfo = TableTopCrucible.Domain.Models.Sources.FileInfo;
using Version = TableTopCrucible.Domain.Models.ValueTypes.Version;

namespace TableTopCrucible.Domain.Library.WPF.ViewModels
{
    public class ItemEditorViewModel : DisposableReactiveValidationObject<ItemEditorViewModel>
    {
        public Control ViewportControl;
        private BehaviorSubject<ItemId?> _selectedItemIdChanges = new BehaviorSubject<ItemId?>(null);
        public IObservable<ItemId?> SelectedItemIdChanges => _selectedItemIdChanges;
        public IObservable<ItemEx?> SelectedItemChanges { get; }
        private readonly IItemService _itemService;
        private readonly INotificationCenterService notificationCenter;
        private readonly ILibraryManagementService libraryManagement;
        private readonly IFileItemLinkService fileItemLinkService;
        private readonly IFileDataService fileDataService;

        public TagEditorViewModel TagEdiotr { get; }
        public FileVersionListViewModel FileVersionList { get; }

        private readonly SourceCache<FileItemLink, FileItemLinkId> newLinks = new SourceCache<FileItemLink, FileItemLinkId>(x => x.Id);


        [Reactive] public ItemEx? SelectedItem { get; set; }
        [Reactive] public string Name { get; set; }
        [Reactive] public Version? SelectedVersion { get; set; }

        public ICommand Save { get; }
        public ICommand CreateThumbnail { get; }

        public ItemEditorViewModel(
            TagEditorViewModel tagEdiotr,
            IItemService itemService,
            FileVersionListViewModel fileVersionList,
            INotificationCenterService notificationCenter,
            ILibraryManagementService libraryManagement,
            IFileItemLinkService fileItemLinkService,
            IFileDataService fileDataService)
        {
            this.TagEdiotr = tagEdiotr;
            this._itemService = itemService;
            FileVersionList = fileVersionList;
            this.notificationCenter = notificationCenter;
            this.libraryManagement = libraryManagement;
            this.fileItemLinkService = fileItemLinkService;
            this.fileDataService = fileDataService;
            this.TagEdiotr.IsEditmode = true;
            this.disposables.Add(_selectedItemIdChanges);

            this.SelectedItemChanges =
                this._itemService
                .GetExtended(this.SelectedItemIdChanges)
                .TakeUntil(destroy);

            this.Save = new RelayCommand(_ => _save());
            this.CreateThumbnail = new RelayCommand(_ => createThumbnail());

            var selectedVersionChanges = this.WhenAnyValue(x => x.SelectedVersion).TakeUntil(destroy);

            var linkFilter = selectedVersionChanges.Select(version =>
                new Func<FileItemLink, bool>(link =>link.Version == version)
            );

            this.newLinks
                .Connect()
                .RemoveKey()
                .Filter(linkFilter)
                .FirstOrDefaultAsync()
                .Select(x => x.FirstOrDefault().Item.Current)
                .Subscribe(x =>
                {

                });

            this.SelectedItemChanges.Subscribe(
                LoadItem,
                ex => this.notificationCenter.OnError(ex));
        }

        private void createThumbnail()
        {
            var dirSetup = SelectedItem?.Directories?.FirstOrDefault(dir => Directory.Exists(dir.Path.LocalPath));
            if (dirSetup == null)
            {
                notificationCenter.OnError(new Exception("no thumbnail-directory found"));
                return;
            }
            var thumbnailDir = dirSetup?.Path.LocalPath + dirSetup?.ThumbnailPath;
            var relativeFilename = @$"{thumbnailDir}\{SelectedItem?.Name}_{DateTime.Now:yyyyMMdd_HHmmssss}.png";
            var relativeUri = new Uri(relativeFilename, UriKind.Relative);
            var absoluteFilename = new Uri(dirSetup?.Path, relativeUri);

            var rect = VisualTreeHelper.GetDescendantBounds(ViewportControl);

            var source = ViewportControl.CreateBitmap(rect.Width, rect.Height, true);

            if (!Directory.Exists(thumbnailDir))
                Directory.CreateDirectory(thumbnailDir);

            using (var fileStream = new FileStream(absoluteFilename.LocalPath, FileMode.Create))
            {
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(source));
                encoder.Save(fileStream);
            };

            var entity = libraryManagement.UpdateFile(dirSetup.Value, relativeUri);

            var oldLink = this.newLinks.KeyValues
                .Select(x => x.Value)
                .FirstOrDefault(link => link.Version == SelectedVersion);

            var changeset = new FileItemLinkChangeset(oldLink)
            {
                ThumbnailKey = entity?.HashKey
            };

            this.newLinks.AddOrUpdate(changeset.Apply());
        }

        private void _save()
        {
            var ics = new ItemChangeset(SelectedItem.Value.SourceItem);
            ics.Name = this.Name;
            ics.Tags = this.TagEdiotr.Tags;
            this._itemService.Patch(ics);

            this.fileItemLinkService.Post(this.newLinks.KeyValues.Select(x => x.Value));
        }
        public void SelectItem(ItemId? id)
            => this._selectedItemIdChanges.OnNext(id);

        public void LoadItem(ItemEx? item)
        {
            if (this.SelectedItem?.SourceItem.Id == item?.SourceItem.Id)
                return;
            try
            {
                this.SelectedItem = item;

                this.Name = (string)item?.Name;
                this.FileVersionList.SetFiles(item?.FileVersions);
                this.TagEdiotr.Tags = item?.Tags;
                this.newLinks.Clear();
                if (item?.FileVersions != null)
                    this.newLinks.AddOrUpdate(item?.FileVersions?.Select(ver => ver.Link.Link));
                this.SelectedVersion = item?.LatestVersion;
            }
            catch (Exception ex)
            {
                this.notificationCenter.OnError(ex);
            }
        }
    }



}
