using System;
using System.Collections.Generic;
using System.Text;

using TableTopCrucible.Domain.Models.ValueTypes;
using TableTopCrucible.Domain.Models.ValueTypes.IDs;

namespace TableTopCrucible.Domain.Models.Sources
{
    public class DirectorySetupChangeset : ReactiveEntityBase<DirectorySetupChangeset, DirectorySetup, DirectorySetupId>, IEntityChangeset<DirectorySetup, DirectorySetupId>
    {
        public DirectorySetupChangeset(DirectorySetup?origin):base(origin)
        { }

        public override IEnumerable<Validator<DirectorySetupChangeset>> Validators { get; }

        public override DirectorySetup Apply() => throw new NotImplementedException();
        public override DirectorySetup ToEntity() => throw new NotImplementedException();
    }
}
