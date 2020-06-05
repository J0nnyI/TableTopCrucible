using System;
using System.Collections.Generic;
using System.Text;

using TableTopCrucible.Domain.Models.Sources;
using TableTopCrucible.Domain.Models.ValueTypes;
using TableTopCrucible.Domain.Models.ValueTypes.IDs;

namespace TableTopCrucible.Domain.Models.Views
{
    public struct ExtendedFileInfo
    {
        public FileInfo FileInfo { get; }
        public DirectorySetup DirectorySetup { get; }

        public Uri RelativePath => FileInfo.Path;
        public Uri RootPath => DirectorySetup.Path;

        public Uri AbsoluteUri { get; }
        public string AbsolutePath => AbsoluteUri.LocalPath;
        public FileHash FileHash => FileInfo.FileHash;
        public DateTime LastWriteTime => FileInfo.LastWriteTime;
        public bool isFileAccessible => FileInfo.isAccessible;
        public bool IsFileNew => FileInfo.IsNew;


        public DirectorySetupName DirectorySetupName => DirectorySetup.Name;
        public Description DirectorySetupDescription => DirectorySetup.Description;



        public ExtendedFileInfo(DirectorySetup directorySetup, FileInfo fileInfo)
        {
            if (directorySetup.Id != fileInfo.DirectorySetupId)
                throw new InvalidOperationException($"{nameof(ExtendedFileInfo)} tried to link dirSetup {directorySetup.Id} with fileInfo {fileInfo} with the dirSetupId {fileInfo.DirectorySetupId}");

            this.FileInfo = fileInfo;
            this.DirectorySetup = directorySetup;
            this.AbsoluteUri = new Uri(directorySetup.Path, fileInfo.Path);
        }
    }
}
