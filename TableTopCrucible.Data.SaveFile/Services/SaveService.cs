using DynamicData;

using ReactiveUI;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Windows;

using TableTopCrucible.Core.Enums;
using TableTopCrucible.Core.Models.Enums;
using TableTopCrucible.Core.Models.Sources;
using TableTopCrucible.Core.Services;
using TableTopCrucible.Data.SaveFile.DataTransferObjects;
using TableTopCrucible.Data.SaveFile.Models;
using TableTopCrucible.Data.SaveFile.Tests.DataTransferObjects;
using TableTopCrucible.Data.Services;
using TableTopCrucible.Data.Models.Sources;
using TableTopCrucible.Domain.Models.ValueTypes;
using FileInfo = TableTopCrucible.Data.Models.Sources.FileInfo;

namespace TableTopCrucible.Data.SaveFile.Services
{

    public class SaveService : ISaveService
    {
        private readonly IItemDataService _itemService;
        private readonly IModelFileDataService _modelFileDataService;
        private readonly IImageFileDataService _imageFileDataService;
        private readonly IFileItemLinkService _fileItemLinkService;
        private readonly IDirectoryDataService _directoryDataService;
        private readonly INotificationCenterService _notificationCenterService;
        private readonly ISettingsService _settingsService;

        public SaveService(
            IItemDataService itemService,
            IModelFileDataService modelFileDataService,
            IImageFileDataService imageFileDataService,
            IFileItemLinkService fileItemLinkService,
            IDirectoryDataService directoryDataService,
            INotificationCenterService notificationCenterService,
            ISettingsService settingsService)
        {
            this._itemService = itemService ?? throw new ArgumentNullException(nameof(itemService));
            this._modelFileDataService = modelFileDataService;
            _imageFileDataService = imageFileDataService;
            this._fileItemLinkService = fileItemLinkService;
            this._directoryDataService = directoryDataService ?? throw new ArgumentNullException(nameof(directoryDataService));
            this._notificationCenterService = notificationCenterService;
            this._settingsService = settingsService;
        }

        public ISaveFileProgression Load(string file)
        {
            var progression = new SaveFileProgression();
            try
            {
                Observable.Start(() => loadFile(file))
                    .Subscribe(dto => applyFile(dto, progression));
            }
            catch (Exception ex)
            {
                progression.OnError(ex);
            }
            return progression;
        }

        private MasterDTO loadFile(string file)
            => JsonSerializer.Deserialize<MasterDTO>(File.ReadAllText(file));

        private ISaveFileProgression applyFile(MasterDTO dto, SaveFileProgression progression)
        {
            Observable.Start(() =>
            {
                try
                {
                    _directoryDataService.Clear();
                    _modelFileDataService.Clear();
                    _fileItemLinkService.Clear();
                    _itemService.Clear();

                    var dir = _directoryDataService.Set(dto.Directories?.Select(dto => dto.ToEntity()) ?? new DirectorySetup[0], null, _settingsService.LoadingPatchSize);
                    dir.Title = "loading directories";
                    progression.DirectoryTaskState = dir;
                    progression.DirectoryTaskState.DoneChanges.Subscribe(
                        dirState =>
                    {
                        if (dirState != TaskState.Done)
                            return;
                        var modelFile = _modelFileDataService.Set(dto.ModelFiles?.Select(dto => dto.ToEntity())??(new FileInfo[0]).AsEnumerable(), null, _settingsService.LoadingPatchSize);
                        modelFile.Title = "loading model files";
                        progression.ModelFileTaskState = modelFile;
                        progression.ModelFileTaskState.DoneChanges.Subscribe(fileState =>
                        {
                            var imageFile = _imageFileDataService.Set(dto.ModelFiles?.Select(dto => dto.ToEntity()) ?? (new FileInfo[0]).AsEnumerable(), null, _settingsService.LoadingPatchSize);
                            imageFile.Title = "loading image files";
                            progression.ImageFileTaskState = imageFile;
                            progression.ImageFileTaskState.DoneChanges.Subscribe(fileState =>
                            {
                                if (fileState != TaskState.Done)
                                    return;
                                var link = _fileItemLinkService.Set(dto.FileItemLinks?.Select(dto => dto.ToEntity()) ?? (new FileItemLink[0]).AsEnumerable(), null, _settingsService.LoadingPatchSize);
                                link.Title = "loading links";
                                progression.LinkTaskState = link;
                                progression.LinkTaskState.DoneChanges.Subscribe(linkState =>
                                {
                                    if (linkState != TaskState.Done)
                                        return;
                                    var item = _itemService.Set(dto.Items?.Select(dto => dto.ToEntity()) ?? (new Item[0]).AsEnumerable(), null, _settingsService.LoadingPatchSize);
                                    item.Title = "loading links";
                                    progression.ItemTaskState = item;
                                    progression.ItemTaskState.DoneChanges.Subscribe(itemState =>
                                    {
                                        if (itemState != TaskState.Done)
                                            return;
                                    });
                                });
                            });
                        });
                    });
                }
                catch (Exception ex)
                {
                    progression.OnError(ex);
                }

            }, RxApp.TaskpoolScheduler);
            return progression;
        }

        public IObservable<Unit> Save(string file)
        {
            if (Path.GetExtension(file) != ".ttcl")
                file = Path.ChangeExtension(file, ".ttcl");

            MasterDTO masterDTO = new MasterDTO()
            {
                Items = _itemService.Get().KeyValues.Select(item => new ItemDTO(item.Value)).ToArray(),
                ModelFiles = _modelFileDataService.Get().KeyValues.Select(file => new FileInfoDTO(file.Value)).ToArray(),
                ImageFiles = _imageFileDataService.Get().KeyValues.Select(file => new FileInfoDTO(file.Value)).ToArray(),
                FileItemLinks = _fileItemLinkService.Get().KeyValues.Select(file => new FileItemLinkDTO(file.Value)).ToArray(),
                Directories = _directoryDataService.Get().KeyValues.Select(dir => new DirectorySetupDTO(dir.Value))
            };
            FileStream fs = File.Create(file);
            var res = JsonSerializer
                .SerializeAsync(fs, masterDTO)
                .ToObservable()
                .Take(1)
                .Catch((Exception ex) =>
                {
                    MessageBox.Show(ex.ToString(), $"{nameof(SaveService)}.{nameof(Save)}:Catch");
                    return Observable.Return(new Unit());
                })
               .Finally(() => fs.Dispose());
            res.Subscribe();
            return res;
        }
    }
}
