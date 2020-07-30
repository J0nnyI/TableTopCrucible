using ReactiveUI;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Windows;

using TableTopCrucible.Core.Models.Enums;
using TableTopCrucible.Core.Models.Sources;
using TableTopCrucible.Core.Services;
using TableTopCrucible.Data.SaveFile.DataTransferObjects;
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
        private readonly IDirectoryDataService _directoryDataService;
        private readonly ISettingsService _settingsService;
        private readonly INotificationCenterService _notificationCenterService;

        public SaveService(
            IItemService itemService,
            IFileDataService _fileDataService,
            IDirectoryDataService directoryDataService,
            ISettingsService settingsService,
            INotificationCenterService notificationCenterService)
        {
            this._itemService = itemService ?? throw new ArgumentNullException(nameof(itemService));
            this._fileDataService = _fileDataService;
            this._directoryDataService = directoryDataService ?? throw new ArgumentNullException(nameof(directoryDataService));
            this._settingsService = settingsService;
            this._notificationCenterService = notificationCenterService;
        }

        public void Load(string file)
        {
            var job = _notificationCenterService.CreateSingleTaskJob(out var proc, $"loading savefile {file}");
            try
            {

                proc.State = AsyncState.InProgress;
                Observable.Start(() => loadFile(file))
                    .Subscribe(dto => applyFile(dto, proc));

            }
            catch (Exception ex)
            {
                proc.Details = ex.ToString();
                proc.State = AsyncState.Failed;
            }
            finally
            {
                job.Dispose();
            }
        }

        private MasterDTO loadFile(string file)
            => JsonSerializer.Deserialize<MasterDTO>(File.ReadAllText(file));

        private void applyFile(MasterDTO dto, AsyncProcessState proc)
        {

            proc.Details = $"loading {dto.Directories.Count()} directories";
            var dirTask = Observable.Start(() => _directoryDataService.Set(dto.Directories.Select(dto => dto.ToEntity())), RxApp.TaskpoolScheduler);


            proc.Details = $"loading {dto.Files.Count()} files";
            var fileTask = Observable.Start(() => _fileDataService.Set(dto.Files.Select(dto => dto.ToEntity())), RxApp.TaskpoolScheduler);


            proc.Details = $"loading {dto.Items.Count()} items";
            var itemTask = Observable.Start(() => _itemService.Set(dto.Items.Select(dto => dto.ToEntity())), RxApp.TaskpoolScheduler);

            Observable.CombineLatest(dirTask, fileTask, itemTask)
                .Take(1)
                .Subscribe(_ =>
                {
                    proc.Details = "done";
                    proc.State = AsyncState.Done;
                },
                ex =>
                {
                    proc.Details = ex.ToString();
                    proc.State = AsyncState.Failed;
                });
        }

        public async void Save(string file)
        {
            MasterDTO masterDTO = new MasterDTO()
            {
                Items = _itemService.Get().KeyValues.Select(item => new ItemDTO(item.Value)).ToArray(),
                Files = _fileDataService.Get().KeyValues.Select(file => new FileInfoDTO(file.Value)).ToArray(),
                Directories = _directoryDataService.Get().KeyValues.Select(dir => new DirectorySetupDTO(dir.Value))
            };
            using FileStream fs = File.Create(file);
            await JsonSerializer.SerializeAsync(fs, masterDTO);
        }
    }
}
