using DynamicData;

using ReactiveUI;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
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
using TableTopCrucible.Domain.Models.Sources;
using TableTopCrucible.Domain.Models.ValueTypes;

namespace TableTopCrucible.Data.SaveFile.Services
{

    public class SaveService : ISaveService
    {
        private readonly IItemService _itemService;
        private readonly IFileDataService _fileDataService;
        private readonly IFileItemLinkService fileItemLinkService;
        private readonly IDirectoryDataService _directoryDataService;
        private readonly INotificationCenterService _notificationCenterService;

        public SaveService(
            IItemService itemService,
            IFileDataService _fileDataService,
            IFileItemLinkService _fileItemLinkService,
            IDirectoryDataService directoryDataService,
            INotificationCenterService notificationCenterService)
        {
            this._itemService = itemService ?? throw new ArgumentNullException(nameof(itemService));
            this._fileDataService = _fileDataService;
            fileItemLinkService = _fileItemLinkService;
            this._directoryDataService = directoryDataService ?? throw new ArgumentNullException(nameof(directoryDataService));
            this._notificationCenterService = notificationCenterService;
        }

        public ISaveFileProgression Load(string file)
        {
            var progression = new SaveFileProgression();
            try
            {
                Observable.Start(() => loadFile(file))
                    .Subscribe(dto => applyFile(dto, progression));
            }
            catch ( Exception ex)
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
                    _fileDataService.Clear();
                    fileItemLinkService.Clear();
                    _itemService.Clear();

                    var dir = _directoryDataService.Set(dto.Directories.Select(dto => dto.ToEntity()),RxApp.TaskpoolScheduler);
                    dir.Title = "loading directories";
                    progression.DirectoryTaskState = dir;
                    progression.DirectoryTaskState.DoneChanges.Subscribe(
                        dirState =>
                    {
                        if (dirState != TaskState.Done)
                            return;
                        var file = _fileDataService.Set(dto.Files.Select(dto => dto.ToEntity()), RxApp.TaskpoolScheduler);
                        file.Title = "loading files";
                        progression.FileTaskState = file;
                        progression.FileTaskState.DoneChanges.Subscribe(fileState =>
                        {
                            if (fileState != TaskState.Done)
                                return;
                            var link = fileItemLinkService.Set(dto.FileItemLinks.Select(dto => dto.ToEntity()), RxApp.TaskpoolScheduler);
                            link.Title = "loading links";
                            progression.LinkTaskState = link;
                            progression.LinkTaskState.DoneChanges.Subscribe(linkState =>
                            {
                                if (linkState != TaskState.Done)
                                    return;
                                var item = _itemService.Set(dto.Items.Select(dto => dto.ToEntity()));
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
                Files = _fileDataService.Get().KeyValues.Select(file => new FileInfoDTO(file.Value)).ToArray(),
                FileItemLinks = fileItemLinkService.Get().KeyValues.Select(file => new FileItemLinkDTO(file.Value)).ToArray(),
                Directories = _directoryDataService.Get().KeyValues.Select(dir => new DirectorySetupDTO(dir.Value))
            };
            FileStream fs = File.Create(file);
            var res = JsonSerializer
                .SerializeAsync(fs, masterDTO)
                .ToObservable()
                .Take(1)
                .Catch((Exception ex) =>
                {
                    MessageBox.Show(ex.ToString(), "saving failed");
                    return Observable.Return(new Unit());
                })
               .Finally(() => fs.Dispose());
            res.Subscribe();
            return res;
        }
    }
}
