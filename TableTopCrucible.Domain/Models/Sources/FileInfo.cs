using System;
using System.Collections.Generic;
using System.Text;

using TableTopCrucible.Domain.Models.ValueTypes;
using TableTopCrucible.Domain.Models.ValueTypes.IDs;

namespace TableTopCrucible.Domain.Models.Sources
{
    public struct FileInfo : IEntity<FileInfoId>
    {
        public FilePath Path { get; }

        public FileInfoId Id { get; }
        public DateTime Created { get; }
        public DateTime LastChange { get; }
    }
}
