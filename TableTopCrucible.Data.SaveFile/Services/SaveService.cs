using ReactiveUI;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
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

        public SaveService(IItemService itemService, IFileDataService _fileDataService, IDirectoryDataService directoryDataService, ISettingsService settingsService, INotificationCenterService notificationCenterService)
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
                MasterDTO masterDTO = JsonSerializer.Deserialize<MasterDTO>(File.ReadAllText(file));

                var size1 = GC.GetTotalMemory(true) / (decimal)1000000;

                proc.Details = $"loading {masterDTO.Directories.Count()} directories";
                _directoryDataService.Set(masterDTO.Directories.Select(dto => dto.ToEntity()));

                var size2 = GC.GetTotalMemory(true) / (decimal)1000000;

                proc.Details = $"loading {masterDTO.Files.Count()} files";
                _fileDataService.Set(masterDTO.Files.Select(dto => dto.ToEntity()));

                var size3 = GC.GetTotalMemory(true) / (decimal)1000000;

                proc.Details = $"loading {masterDTO.Items.Count()} items";
                _itemService.Set(masterDTO.Items.Select(dto => dto.ToEntity()));

                var size4 = GC.GetTotalMemory(true) / (decimal)1000000;

                proc.Details = "done";
                proc.State = AsyncState.Done;

                MessageBox.Show($"before: {size1}mb after:{size4}mb{Environment.NewLine}dir size: {size2 - size1}mb{Environment.NewLine}file size: {size3 - size2}mb{Environment.NewLine}itemSize: {size4 - size3}mb");
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
