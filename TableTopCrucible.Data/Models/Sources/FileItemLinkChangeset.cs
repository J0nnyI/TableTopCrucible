﻿using System;
using System.Collections.Generic;

using TableTopCrucible.Core.Models.Sources;
using TableTopCrucible.Domain.Models.ValueTypes;
using TableTopCrucible.Domain.Models.ValueTypes.IDs;

using Version = TableTopCrucible.Domain.Models.ValueTypes.Version;

namespace TableTopCrucible.Domain.Models.Sources
{
    public struct FileItemLinkChangeset : IEntityChangeset<FileItemLink, FileItemLinkId>
    {
        public ItemId ItemId { get; set; }

        public FileInfoHashKey FileKey { get; set; }

        public Version Version { get; set; }
        public FileItemLink? Origin { get; }

        public FileItemLink Apply() => new FileItemLink();
        public IEnumerable<string> GetErrors() => throw new NotImplementedException();
        public FileItemLink ToEntity() => throw new NotImplementedException();
    }
}