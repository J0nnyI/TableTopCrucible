using DynamicData;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using System;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
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
using TableTopCrucible.Domain.Models.ValueTypes.IDs;
using TableTopCrucible.WPF.Commands;
using TableTopCrucible.Core.Helper;
using Version = TableTopCrucible.Domain.Models.ValueTypes.Version;
using TableTopCrucible.Core.WPF.Commands;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;
using TableTopCrucible.Data.Models.ValueTypes;
using System.Collections.Generic;
using TableTopCrucible.FeatureCore.WPF.Tagging.ViewModels;

namespace TableTopCrucible.Domain.Library.WPF.ViewModels
{
    public class ItemEditorViewModel : DisposableReactiveValidationObject
    {
        public Control ViewportControl;
        private BehaviorSubject<ItemId?> _selectedItemIdChanges = new BehaviorSubject<ItemId?>(null);
        public IObservable<ItemId?> SelectedItemIdChanges => _selectedItemIdChanges;
        public IObservable<ItemEx?> SelectedItemChanges { get; }
        private readonly IItemDataService _itemService;
        private readonly INotificationCenterService notificationCenter;
        private readonly ILibraryManagementService libraryManagement;
        private readonly IFileItemLinkService fileItemLinkService;
        private readonly IThumbnailManagementService thumbnailManagementService;

        public IManualTagEditor TagEditor { get; }
        public FileVersionListViewModel FileVersionList { get; }
        public ICommand OpenFile { get; }
        public FileToClipboardCommand FileToClipboard { get; }

        private readonly SourceCache<FileItemLink, FileItemLinkId> newLinks = new SourceCache<FileItemLink, FileItemLinkId>(x => x.Id);


        [Reactive] public ItemEx? SelectedItem { get; set; }
        [Reactive] public string Name { get; set; }
        [Reactive] public Version? SelectedVersion { get; set; }
        [Reactive] public Model3DGroup ViewportContent { get; set; }
        [Reactive] public bool LoadingModel { get; set; } = false;
        [Reactive] public SolidColorBrush MaterialBrush { get; private set; }



        public readonly ObservableAsPropertyHelper<VersionedFile?> _selectedFiles;
        public VersionedFile? SelectedFiles => _selectedFiles.Value;

        public ICommand Save { get; }
        public ICommand CreateThumbnail { get; }

        private readonly ObservableAsPropertyHelper<CameraRotationMode> _cameraRotationMode;
        public CameraRotationMode CameraRotationMode => _cameraRotationMode.Value;

        public ItemEditorViewModel(
            IManualTagEditor tagEdiotr,
            IItemDataService itemService,
            FileVersionListViewModel fileVersionList,
            INotificationCenterService notificationCenter,
            ILibraryManagementService libraryManagement,
            IFileItemLinkService fileItemLinkService,
            OpenFileCommand openFile,
            FileToClipboardCommand fileToClipboard,
            ISettingsService settingsService,
            IThumbnailManagementService thumbnailManagementService)
        {
            this.TagEditor = tagEdiotr;
            this._itemService = itemService;
            FileVersionList = fileVersionList;
            this.notificationCenter = notificationCenter;
            this.libraryManagement = libraryManagement;
            this.fileItemLinkService = fileItemLinkService;
            OpenFile = openFile;
            FileToClipboard = fileToClipboard;
            this.thumbnailManagementService = thumbnailManagementService;
            this.TagEditor.Editmode = true;
            this.TagEditor.CompletePool = true;
            this.TagEditor.PermitNewTags = true;
            this.disposables.Add(_selectedItemIdChanges);

            this.SelectedItemChanges =
                this._itemService
                .GetExtended(this.SelectedItemIdChanges)
                .TakeUntil(destroy);

            this.Save = new RelayCommand(_ => _save());
            this.CreateThumbnail = new RelayCommand(_ => createThumbnail());

            var selectedVersionChanges = this.WhenAnyValue(x => x.SelectedVersion).TakeUntil(destroy);

            var curLink = this.newLinks.Connect()
                .ChangeKey(link => link.Version)
                .WatchValue(selectedVersionChanges, link => link.Version);

            var curFileFilter =
                selectedVersionChanges.CombineLatest(SelectedItemChanges, (version, item) => new { version, item })
                .Select(
                x =>
                {
                    return new Func<VersionedFile, bool>((VersionedFile vFile)
                        => x.version.HasValue && x.item.HasValue && (vFile.Version == x.version.Value) && (vFile.ItemId == x.item.Value.ItemId));
                });

            var files = this.fileItemLinkService.BuildversionedFiles(newLinks.Connect());

            var curFiles = files.Filter(curFileFilter)
                .Select(x => x.FirstOrDefault(x => x.Reason != ChangeReason.Remove).Current.ToNullable());

            this._selectedFiles = curFiles
                .TakeUntil(destroy)
                .ToProperty(this, nameof(SelectedFiles));

            this._cameraRotationMode = settingsService
                .WhenAnyValue(x => x.CameraRotationMode)
                .Select(x => (CameraRotationMode)x)
                .ToProperty(this, nameof(CameraRotationMode));
            _cameraRotationMode.DisposeWith(disposables);

            DateTime mostRecentRequest = default;
            curFiles
                .ObserveOn(RxApp.TaskpoolScheduler)
                .TakeUntil(destroy)
                .Select(files =>
                {
                    mostRecentRequest = DateTime.Now;
                    var orderTime = DateTime.Now;
                    if (!files.HasValue || files?.File.AbsolutePath == null)
                        return null;
                    this.MaterialBrush = new SolidColorBrush(Colors.LightGray);
                    var mat = new DiffuseMaterial(this.MaterialBrush);
                    ModelImporter importer = new ModelImporter()
                    {
                        DefaultMaterial = mat
                    };
                    Model3DGroup model = importer.Load(files.Value.File.AbsolutePath);
                    model.SetMaterial(mat);
                    model.PlaceAtOrigin();
                    model.Freeze();
                    return new { timestamp = mostRecentRequest, model };
                })
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x =>
                {
                    this.ViewportContent = null;
                    if (x == null)
                        return;
                    LoadingModel = true;
                    RxApp.MainThreadScheduler.Schedule(x.model, (_, model) =>
                    {
                        if (x.timestamp == mostRecentRequest)
                        {
                            this.ViewportContent = model;
                            LoadingModel = false;
                        }
                        return null;
                    });
                });

            var linkFilter = selectedVersionChanges.Select(version =>
                new Func<FileItemLink, bool>(link => link.Version == version)
            );

            var selectedLinkChanges = this.newLinks
                .Connect()
                .RemoveKey()
                .Filter(linkFilter)
                .Select(x => x.FirstOrDefault().Item.Current);



            this.SelectedItemChanges.Subscribe(
                LoadItem,
                ex => this.notificationCenter.OnError(ex));
        }


        private void createThumbnail()
        {
            if (!SelectedItem.HasValue)
                return;

            try
            {

                var oldLink = this.newLinks.KeyValues
                    .Select(x => x.Value)
                    .FirstOrDefault(link => link.Version == SelectedVersion);

                thumbnailManagementService.CreateAndLinkThumbnail(SelectedItem.Value, fs =>
                {
                    var ratio = ViewportControl.ActualHeight / ViewportControl.ActualWidth;
                    var res = 400;

                    var source = VisualUtility.CreateBitmap(ViewportControl, res, res * ratio);

                    BitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(source));
                    encoder.Save(fs);
                    fs.Close();
                }, out var file, out var link, oldLink);

                this.newLinks.AddOrUpdate(link.Apply());
            }
            catch (Exception ex)
            {
                notificationCenter.CreateSingleTaskJob(out var proc, "thumbnail creation failed");
                proc.Title = "exception: ";
                proc.Details = ex.ToString();
            }
        }

        private void _save()
        {
            var ics = new ItemChangeset(SelectedItem.Value.SourceItem);
            ics.Name = this.Name;
            ics.Tags = this.TagEditor.Selection.Items;
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
                if (this.SelectedItem != null)
                    this._save();

                LoadingModel = true;
                this.ViewportContent = null;

                this.SelectedItem = item;

                this.Name = (string)item?.Name;
                this.FileVersionList.SetFiles(item?.FileVersions);
                this.TagEditor.SetSelection(item?.Tags);
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

        public void SetTagpool(IObservableList<Tag> tagpool)
            => this.TagEditor.SetTagpool(tagpool);
    }
}
