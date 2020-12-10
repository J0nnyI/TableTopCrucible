using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using TableTopCrucible.Data.Models.Sources;
using TableTopCrucible.Domain.Models.ValueTypes;
using TableTopCrucible.Domain.Models.ValueTypes.IDs;

using FileInfo = TableTopCrucible.Data.Models.Sources.FileInfo;

namespace TableTopCrucible.Data.Models.Views
{
    public struct FileInfoEx
    {
        public FileInfo FileInfo { get; }
        public DirectorySetup DirectorySetup { get; }

        public Uri RelativePath => FileInfo.Path;
        public Uri RootPath => DirectorySetup.Path;

        public Uri AbsoluteUri { get; }
        public string AbsolutePath => AbsoluteUri?.LocalPath;
        public string SafeAbsolutePath => File.Exists(AbsolutePath) ? AbsolutePath : null;
        public FileHash? FileHash => FileInfo.FileHash;
        public DateTime LastWriteTime => FileInfo.LastWriteTime;
        public bool IsFileAccessible => FileInfo.IsAccessible;
        public int Hash => FileInfo.HashKey.GetHashCode();
        public FileInfoHashKey? HashKey => FileInfo.HashKey;
        public FileInfoId Id => FileInfo.Id;

        public DirectorySetupName DirectorySetupName => DirectorySetup.Name;
        public Description DirectorySetupDescription => DirectorySetup.Description;



        public FileInfoEx(DirectorySetup directorySetup, FileInfo fileInfo)
        {
            if (directorySetup.Id != fileInfo.DirectorySetupId)
                throw new InvalidOperationException($"{nameof(FileInfoEx)} tried to link dirSetup {directorySetup.Id} with fileInfo {fileInfo} with the dirSetupId {fileInfo.DirectorySetupId}");

            FileInfo = fileInfo;
            DirectorySetup = directorySetup;
            AbsoluteUri = new Uri(directorySetup.Path, fileInfo.Path);
        }


        public static bool operator ==(FileInfoEx fileA, FileInfoEx fileB)
            => fileA.FileInfo == fileB.FileInfo && fileA.DirectorySetup == fileB.DirectorySetup;
        public static bool operator !=(FileInfoEx fileA, FileInfoEx fileB)
            => fileA.FileInfo.Identity != fileB.FileInfo.Identity || fileA.FileInfo.Identity != fileB.FileInfo.Identity;

        private class ComparerClass : IEqualityComparer<FileInfoEx>, IEqualityComparer
        {
            bool IEqualityComparer.Equals(object x, object y)
                => x is FileInfoEx ex && y is FileInfoEx ey && Equals(ex, ey);
            public bool Equals(FileInfoEx x, FileInfoEx y)
                => x == y;
            int IEqualityComparer.GetHashCode(object obj) => obj.GetHashCode();
            public int GetHashCode(FileInfoEx obj) => obj.GetHashCode();
        }
        public static IEqualityComparer<FileInfoEx> Comparer { get; } = new ComparerClass();

        public override bool Equals(object obj)
        {
            if (obj is FileInfoEx fileInfo)
                return fileInfo == this;
            return false;
        }
        public override int GetHashCode()
            => HashCode.Combine(FileInfo, DirectorySetup);
    }

}
