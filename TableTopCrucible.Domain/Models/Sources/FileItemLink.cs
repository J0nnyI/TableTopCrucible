using System;
using Version = TableTopCrucible.Domain.Models.ValueTypes.Version;
using System.Collections.Generic;
using System.Text;

using TableTopCrucible.Domain.Models.ValueTypes;
using TableTopCrucible.Domain.Models.ValueTypes.IDs;

namespace TableTopCrucible.Domain.Models.Sources
{
    public struct FileItemLink : IEntity<FileItemLinkId>
    {
        public FileItemLinkId Id { get; }
        public ItemId ItemId { get; }

        public FileHash FileHash { get; }
        public int FileSize { get; }

        public Version Version { get; }

        public DateTime Created { get; }
        public DateTime LastChange { get; }
    }
}
