using DynamicData;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;

using TableTopCrucible.Domain.Models.Sources;
using TableTopCrucible.Domain.Models.ValueTypes.IDs;
using TableTopCrucible.Domain.Models.Views;

using FileInfo = TableTopCrucible.Domain.Models.Sources.FileInfo;
using SysFileInfo = System.IO.FileInfo;

namespace TableTopCrucible.Domain.Services
{
    public interface IFileInfoService : IDataService<FileInfo, FileInfoId, FileInfoChangeset>
    {
        void Synchronize();
        public IObservableCache<ExtendedFileInfo, FileInfoId> GetFullFIleInfo();
    }
    public class FileInfoService : DataServiceBase<FileInfo, FileInfoId, FileInfoChangeset>, IFileInfoService
    {
        private bool synchronizing = false;
        private IDirectorySetupService _directorySetupService;
        public FileInfoService(IDirectorySetupService directorySetupService)
        {
            this._directorySetupService = directorySetupService;
        }


        public IObservableCache<ExtendedFileInfo, FileInfoId> GetFullFIleInfo() =>
        this._directorySetupService
            .Get()
            .Connect()
            .LeftJoinMany(
            this.cache.Connect(),
            (FileInfo dirSetup) => dirSetup.DirectorySetupId,
                (left, right) => right.Items.Select(item => new ExtendedFileInfo(left, item))
            )
            .TransformMany(x => x, x => x.FileInfo.Id)
            .TakeUntil(destroy)
            .AsObservableCache();

        public void Synchronize()
        {
            if (synchronizing)
                return;
            synchronizing = true;

            IEnumerable<DirectorySetup> dirSetups = this._directorySetupService
                .Get()
                .KeyValues
                .Select(x => x.Value);

            IEnumerable<ExtendedFileInfo> fileInfos = this.GetFullFIleInfo().KeyValues.Select(x => x.Value);

            var actualDirSetupFiles = dirSetups
                .Where(dirSetup => dirSetup.IsValid)
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

            IEnumerable<FileInfoChangeset> mergedFiles =
                from file in allPaths
                join foundFile in flatDirSetupFiles
                    on file.path equals foundFile.path into foundFiles
                join definedFile in fileInfos
                    on file.path equals definedFile.AbsolutePath into definedFiles

                select new FileInfoChangeset(definedFiles.Any() ? definedFiles.First().FileInfo as FileInfo? : null)
                {
                    Path =
                        new Uri(Uri.UnescapeDataString(
                            (
                            definedFiles.Any()
                                ? definedFiles.First().DirectorySetup
                                : foundFiles.First().dirSetup
                            ).Path.MakeRelativeUri(new Uri(file.path))
                            .ToString()
                        ), UriKind.Relative),
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
