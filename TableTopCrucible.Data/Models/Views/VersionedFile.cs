using System;
using System.Collections.Generic;
using System.Text;

using TableTopCrucible.Domain.Models.Sources;

namespace TableTopCrucible.Data.Models.Views
{
    public struct VersionedFile
    {

        public VersionedFile( FileItemLink link,IEnumerable<ExtendedFileInfo> files)
        {
            Link = link;
            Files = files;
        }

        public FileItemLink Link { get; }
        public IEnumerable<ExtendedFileInfo> Files { get; }
    }
}
