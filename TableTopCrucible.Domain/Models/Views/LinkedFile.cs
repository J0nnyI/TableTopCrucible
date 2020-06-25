using System;
using System.Collections.Generic;
using System.Text;

using TableTopCrucible.Domain.Models.Sources;
using TableTopCrucible.Domain.Models.ValueTypes;

namespace TableTopCrucible.Domain.Models.Views
{
    public struct LinkedFile
    {
        public ExtendedFileInfo File { get; }
        public FileItemLink Link { get; }
        public LinkedFile(ExtendedFileInfo  file, FileItemLink link)
        {
            if (new FileInfoHashKey(file.FileInfo) == link.FileKey)
                throw new InvalidOperationException($"File {file.AbsolutePath} is not linked via {link}");
            this.File = file;
            this.Link = link;
        }
    }
}
