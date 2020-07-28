using ReactiveUI;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;

using TableTopCrucible.Core.Models.Enums;
using TableTopCrucible.Core.Services;
using TableTopCrucible.Data.Services;
using TableTopCrucible.Domain.Models.Sources;
using TableTopCrucible.Domain.Models.Views;
using TableTopCrucible.WPF.Helper;

using SysFileInfo = System.IO.FileInfo;
namespace TableTopCrucible.Domain.Library
{
    public interface ILibraryManagementService
    {
        void SynchronizeFiles();
    }

    public class LibraryManagementService : ILibraryManagementService
    {
        private struct LocalFile
        {
            public LocalFile(DirectorySetup directorySetup, string localPath)
            {
                DirectorySetup = directorySetup;
                FileInfo = new SysFileInfo(localPath);
            }

            public DirectorySetup DirectorySetup { get; }
            public SysFileInfo FileInfo { get; }
        }

        private readonly IDirectoryDataService directoryDataService;
        private readonly IFileDataService fileDataService;
        private readonly IItemService itemService;
        private readonly INotificationCenterService notificationCenter;

        public LibraryManagementService(IDirectoryDataService directoryDataService, IFileDataService fileDataService, IItemService itemService, INotificationCenterService notificationCenter)
        {
            this.directoryDataService = directoryDataService;
            this.fileDataService = fileDataService;
            this.itemService = itemService;
            this.notificationCenter = notificationCenter;
        }

        private IEnumerable<ExtendedFileInfo> getDefinedFiles()
        {
            return fileDataService
                .GetExtended()
                .KeyValues
                .Select(x => x.Value);
        }

        private IEnumerable<LocalFile> getLocalFiles(DirectorySetup directorySetup)
        {
            return Directory.GetFiles(directorySetup.Path.LocalPath)
                .Select(file => new LocalFile(directorySetup, file));
        }

        private IEnumerable<LocalFile> getLocalFiles(IEnumerable<DirectorySetup> dirSetups)
            => dirSetups.SelectMany(setup => getLocalFiles(setup));

        private IEnumerable<string> getDistinctPaths(IEnumerable<LocalFile> localFiles, IEnumerable<ExtendedFileInfo> definedFiles)
        {
            var localPaths = localFiles.Select(file => file.FileInfo.FullName);
            var definedPaths = definedFiles.Select(file => file.AbsolutePath);
            return localPaths.Concat(definedPaths).Distinct();
        }

        private IEnumerable<DirectorySetup> getDirSetups()
            => this.directoryDataService.Get().KeyValues.Select(x => x.Value);
        private FileInfoChangeset synchronizeFile(LocalFile? localFile, ExtendedFileInfo? definedFile)
        {
            if (localFile.HasValue && definedFile.HasValue)
            {
                if (localFile?.DirectorySetup != definedFile?.DirectorySetup)
                    throw new Exception($"dir setups do not match up for file '{localFile?.FileInfo?.FullName}' / '{definedFile?.AbsolutePath}'");
                if (localFile?.FileInfo?.FullName != definedFile?.AbsolutePath)
                    throw new Exception($"path do not match up for file '{localFile?.FileInfo?.FullName}' / '{definedFile?.AbsolutePath}'");
            }


            var result = new FileInfoChangeset(definedFile?.FileInfo);
            var dirSetup = (definedFile?.DirectorySetup ?? localFile?.DirectorySetup).Value;

            // no filechange
            if (localFile.HasValue && definedFile.HasValue && localFile?.FileInfo?.LastWriteTime == definedFile?.LastWriteTime)
                return null;

            // file was deleted
            result.IsAccessible = localFile.HasValue;

            var path = definedFile?.AbsolutePath ?? localFile?.FileInfo?.FullName;

            result.SetSysFileInfo(dirSetup, localFile?.FileInfo);
            result.Path = dirSetup.Path.MakeUnescapedRelativeUri(path);
            result.DirectorySetupId = localFile?.DirectorySetup.Id ?? definedFile.Value.DirectorySetup.Id;

            return result;
        }
        public void SynchronizeFiles()
        {
            notificationCenter.CreateSingleTaskJob(out var processState, $"working...", "locating files");
            try
            {
                processState.State = AsyncState.InProgress;
                var dirSetups = getDirSetups();
                var definedFiles = getDefinedFiles().ToArray();
                var localFiles = getLocalFiles(dirSetups).ToArray();
                var paths = getDistinctPaths(localFiles, definedFiles);

                var mergedFiles =
                    (from file in paths
                     join localFile in localFiles
                         on file equals localFile.FileInfo.FullName into mLocalFile
                     join definedFile in definedFiles
                         on file equals definedFile.AbsolutePath into mDefinedFile
                     select synchronizeFile(mLocalFile.Any() ? mLocalFile.First() as LocalFile? : null, mDefinedFile.Any() ? mDefinedFile.First() as ExtendedFileInfo? : null))
                    .Where(x => x != null);

                var changesets = mergedFiles.Where(x => x.IsAccessible).ToArray();
                var deleteSets = mergedFiles.Where(x => !x.IsAccessible).Select(x => x.Id).ToArray();
                processState.Details = $"patching: located {changesets.Length} files, detected {deleteSets.Length} deleted files";
                fileDataService.Patch(changesets);
                fileDataService.Delete(deleteSets);
                processState.State = AsyncState.Done;
                processState.Details = $"done. located {changesets.Length} files, detected {deleteSets.Length} deleted files";
            }
            catch (Exception ex)
            {
                processState.State = AsyncState.Failed;
                processState.Details = ex.ToString();
            }
        }
    }
}
