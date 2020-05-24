using System;
using System.Collections.Generic;
using System.Text;

using TableTopCrucible.Domain.Models.ValueTypes.IDs;

namespace TableTopCrucible.Domain.Models.Sources
{
    public class FileInfoChangeset : EntityChangesetBase<FileInfo, FileInfoId>, IEntityChangeset<FileInfo, FileInfoId>
    {
        public FileInfoChangeset(FileInfo? origin = null) : base(origin)
        {
        }

        public override FileInfo Apply() => throw new NotImplementedException();
        public override IEnumerable<string> GetErrors() => throw new NotImplementedException();
        public override FileInfo ToEntity() => throw new NotImplementedException();
    }
}
