using System;
using System.Collections.Generic;
using System.Text;

using TableTopCrucible.Domain.Models.ValueTypes.IDs;

namespace TableTopCrucible.Domain.Models.Sources
{
    public struct DirectorySetup : IEntity<DirectorySetupId>
    {
        public DirectorySetupId Id { get; }
        public DateTime Created { get; }
        public DateTime LastChange { get; }
    }
}
