using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Navigation;

using TableTopCrucible.Domain.Models.ValueTypes;
using TableTopCrucible.Domain.Models.ValueTypes.IDs;

namespace TableTopCrucible.Domain.Models.Sources
{
    public class DirectorySetupChangeset : ReactiveEntityBase<DirectorySetupChangeset, DirectorySetup, DirectorySetupId>, IEntityChangeset<DirectorySetup, DirectorySetupId>
    {
        public string Path { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }


        public DirectorySetupChangeset(DirectorySetup? origin = null) : base(origin)
        { }

        public override IEnumerable<Validator<DirectorySetupChangeset>> Validators { get; }

        public override DirectorySetup Apply()
            => new DirectorySetup(this.Origin.Value, new Uri(Path), (DirectorySetupName)Name, (Description)Description);
        public override DirectorySetup ToEntity()
            => new DirectorySetup(new Uri(Path), (DirectorySetupName)Name, (Description)Description);


    }
}
