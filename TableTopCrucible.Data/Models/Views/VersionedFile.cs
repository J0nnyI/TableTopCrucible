using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using TableTopCrucible.Domain.Models.Sources;
using TableTopCrucible.Domain.Models.ValueTypes;

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
        public FileInfoEx File => Files.FirstOrDefault(x => System.IO.File.Exists(x.AbsolutePath));
        public Version Version => Link.Version;
        public FileInfoEx? Thumbnail => Link.Thumbnail;
    }
}
