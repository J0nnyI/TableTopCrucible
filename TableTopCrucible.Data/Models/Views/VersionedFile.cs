using System.Collections.Generic;
using System.Text;

using TableTopCrucible.Domain.Models.Sources;
using TableTopCrucible.Domain.Models.ValueTypes;

namespace TableTopCrucible.Data.Models.Views
{
    public struct VersionedFile
    {

        public VersionedFile( FileItemLinkEx link,IEnumerable<FileInfoEx> files)
        {
            Link = link;
            Files = files;
        }

        public FileItemLinkEx Link { get; }
        public IEnumerable<FileInfoEx> Files { get; }

        public Version Version => Link.Version;
        public FileInfoEx? Thumbnail=> Link.Thumbnail;
    }
}
