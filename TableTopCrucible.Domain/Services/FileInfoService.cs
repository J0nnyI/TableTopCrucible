using System;
using System.Collections.Generic;
using System.IO;
using SysFileInfo = System.IO.FileInfo;
using System.Text;

using TableTopCrucible.Domain.Models.Sources;
using FileInfo = TableTopCrucible.Domain.Models.Sources.FileInfo;
using TableTopCrucible.Domain.Models.ValueTypes.IDs;
using System.Dynamic;
using DynamicData;
using System.Reactive.Linq;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using System.Reactive;
using TableTopCrucible.Domain.Models;
using ReactiveUI;
using System.Windows.Navigation;
using System.Security.Cryptography;
using TableTopCrucible.Domain.Models.ValueTypes;
using TableTopCrucible.Domain.Models.Views;

namespace TableTopCrucible.Domain.Services
{
    public interface IFileInfoService : IDataService<FileInfo, FileInfoId, FileInfoChangeset>
    {
        void Synchronize();
    }
    public class FileInfoService : DataServiceBase<FileInfo, FileInfoId, FileInfoChangeset>, IFileInfoService
    {
        private bool synchronizing = false;
        private IDirectorySetupService _directorySetupService;
        public FileInfoService(IDirectorySetupService directorySetupService)
        {
            this._directorySetupService = directorySetupService;
        }


        public IObservableCache<ExtendedFileInfo, DirectorySetupId> FullFIleInfo =>

            this._directorySetupService.Get().Connect().LeftJoin
                (
                this.cache.Connect(),
                (FileInfo dirSetup) => dirSetup.DirectorySetupId,
                (left, right) => new ExtendedFileInfo(left, right.Value)
                )
                .TakeUntil(destroy)
                .AsObservableCache();

        public async void Synchronize()
        {
            if (synchronizing)
                return;
            synchronizing = true;

            IEnumerable<DirectorySetup> dirSetups = this._directorySetupService
                .Get()
                .KeyValues
                .Select(x => x.Value);

            IEnumerable<ExtendedFileInfo> fileInfos = this.FullFIleInfo.KeyValues.Select(x => x.Value);

            var actualDirSetupFiles = dirSetups
                .Select(dirSetup => new { files = Directory.GetFiles(dirSetup.Path.LocalPath, "*", SearchOption.AllDirectories), dirSetup }).ToArray();

            var flatDirSetupFiles = actualDirSetupFiles
                .SelectMany(files => files.files.Select(path => new { files.dirSetup, path }));

            IEnumerable<string> actualFiles = actualDirSetupFiles
                .SelectMany(dirSetupFiles => dirSetupFiles.files);

            var allPaths = fileInfos
                    .Select(x => x.AbsolutePath)
                    .Union(actualFiles)
                    .Distinct()
                    .Select(path => new { path, info = new SysFileInfo(path) });


            var mergedFiles =
                from file in allPaths
                join foundFile in flatDirSetupFiles
                    on file.path equals foundFile.path into foundFiles
                join definedFile in fileInfos
                    on file.path equals definedFile.AbsolutePath into definedFiles

                select new FileInfoChangeset(definedFiles.Any() ? definedFiles.First().FileInfo as FileInfo? : null)
                {
                    Path = (
                        definedFiles.Any()
                            ? definedFiles.First().DirectorySetup
                            : foundFiles.First().dirSetup
                        ).Path.MakeRelativeUri(new Uri(file.path)),
                    CreationTime = file.info.CreationTime,
                    LastWriteTime = file.info.LastWriteTime,
                    IsAccessible = !foundFiles.Any(),
                    IsNew = !definedFiles.Any(),
                    DirectorySetupId = foundFiles.Any() ? foundFiles.First().dirSetup.Id : definedFiles.First().DirectorySetup.Id
                };

            this.Patch(mergedFiles);
        }

    }
}
