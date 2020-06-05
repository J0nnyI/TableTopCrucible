using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Security.Policy;
using System.Text;

using TableTopCrucible.Domain.Models.ValueTypes;
using TableTopCrucible.Domain.Models.ValueTypes.IDs;

namespace TableTopCrucible.Domain.Models.Sources
{
    public class FileInfoChangeset : EntityChangesetBase<FileInfo, FileInfoId>, IEntityChangeset<FileInfo, FileInfoId>
    {
        [Reactive]
        public Uri Path { get; set; }
        
        [Reactive]
        public DateTime CreationTime { get; set; }

        [Reactive] 
        public FileHash FileHash { get; set; }

        [Reactive]
        public DateTime LastWriteTime { get; set; }

        [Reactive]
        public DirectorySetupId DirectorySetupId { get; set; }

        [Reactive]
        public bool IsAccessible { get; set; }

        [Reactive]
        public bool IsNew { get; set; }



        public FileInfoChangeset(FileInfo? origin = null) : base(origin)
        {
            if(origin.HasValue){

            }
        }

        public override FileInfo Apply()
        {
            return new FileInfo(this.Origin.Value, Path, CreationTime, FileHash, LastWriteTime, DirectorySetupId, IsAccessible, IsNew);
        }
        public override IEnumerable<string> GetErrors() => throw new NotImplementedException();
        public override FileInfo ToEntity() => throw new NotImplementedException();
    }
}
