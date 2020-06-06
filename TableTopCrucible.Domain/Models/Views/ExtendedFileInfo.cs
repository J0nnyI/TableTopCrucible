using System;
using System.Collections;
using System.Collections.Generic;

using TableTopCrucible.Domain.Models.Sources;
using TableTopCrucible.Domain.Models.ValueTypes;

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


        public static bool operator ==(ExtendedFileInfo fileA, ExtendedFileInfo fileB)
            => fileA.FileInfo == fileB.FileInfo && fileA.DirectorySetup == fileB.DirectorySetup;
        public static bool operator !=(ExtendedFileInfo fileA, ExtendedFileInfo fileB)
            => fileA.FileInfo.Identity != fileB.FileInfo.Identity || fileA.FileInfo.Identity != fileB.FileInfo.Identity;

        private class ComparerClass : IEqualityComparer<ExtendedFileInfo>, IEqualityComparer
        {
            bool IEqualityComparer.Equals(object x, object y)
                => x is ExtendedFileInfo ex && y is ExtendedFileInfo ey && this.Equals(ex, ey);
            public bool Equals(ExtendedFileInfo x, ExtendedFileInfo y)
                => x == y;
            int IEqualityComparer.GetHashCode(object obj) => obj.GetHashCode();
            public int GetHashCode(ExtendedFileInfo obj) => obj.GetHashCode();
        }
        public static IEqualityComparer<ExtendedFileInfo> Comparer { get; } = new ComparerClass();

        public override bool Equals(object obj)
            => obj is ExtendedFileInfo info && EqualityComparer<FileInfo>.Default.Equals(this.FileInfo, info.FileInfo) && EqualityComparer<DirectorySetup>.Default.Equals(this.DirectorySetup, info.DirectorySetup);
        public override int GetHashCode()
            => HashCode.Combine(this.FileInfo, this.DirectorySetup);
    }

}
