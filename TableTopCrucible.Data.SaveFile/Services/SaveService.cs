using DynamicData;

using ReactiveUI;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
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
            Observable.Start(() =>
            {
                try
                {
                    _directoryDataService.Clear();
                    _fileDataService.Clear();
                    fileItemLinkService.Clear();
                    _itemService.Clear();


                    proc.AddProgress(4, $"loading {dto.Directories.Count()} directories");
                    _directoryDataService.Set(dto.Directories.Select(dto => dto.ToEntity()));

                    proc.OnNextStep($"loading {dto.Files.Count()} files");
                    _fileDataService.Set(dto.Files.Select(dto => dto.ToEntity()));

                    proc.OnNextStep($"loading {dto.Items.Count()} file-item links");
                    fileItemLinkService.Set(dto.FileItemLinks.Select(dto => dto.ToEntity()));

                    proc.OnNextStep($"loading {dto.Items.Count()} items");
                    _itemService.Set(dto.Items.Select(dto => dto.ToEntity()));

                    proc.OnNextStep("done");
                    proc.State = AsyncState.Done;
                }
                catch (Exception ex)
                {
                    proc.Details = ex.ToString();
                    proc.State = AsyncState.Failed;
                }


            }, RxApp.TaskpoolScheduler);
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
