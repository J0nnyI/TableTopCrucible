using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using TableTopCrucible.Domain.Models.Sources;
using TableTopCrucible.Domain.Models.ValueTypes;
using TableTopCrucible.Domain.Models.ValueTypes.IDs;

namespace TableTopCrucible.Data.Models.Views
{
    public struct VersionedFile
    {

        public VersionedFile(FileItemLinkEx link, IEnumerable<FileInfoEx> files)
        {
            Link = link;
            Files = files;
        }

        public FileItemLinkEx Link { get; }
        public IEnumerable<FileInfoEx> Files { get; }
        public FileInfoHashKey HashKey => Link.FileKey;
        public ItemId ItemId => Link.ItemId;
        public FileInfoEx File => Files.FirstOrDefault(x => System.IO.File.Exists(x.AbsolutePath));
        public IEnumerable<string> FilePaths => Files.Select(file => file.AbsolutePath);
        public Version Version => Link.Version;
        public FileInfoEx? Thumbnail => Link.Thumbnail;
    }
}
