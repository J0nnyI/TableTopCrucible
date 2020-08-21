
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;

using TableTopCrucible.Core.Models.Sources;
using TableTopCrucible.Core.Models.ValueTypes;
using TableTopCrucible.Core.Utilities;
using TableTopCrucible.Domain.Models.ValueTypes;
using TableTopCrucible.Domain.Models.ValueTypes.IDs;

namespace TableTopCrucible.Domain.Models.Sources
{
    public class ItemChangeset : ReactiveEntityBase<ItemChangeset, Item, ItemId>, IEntityChangeset<Item, ItemId>
    {
        public IObservable<string> NameChanges { get; }
        [Reactive] public string Name { get; set; }
        public IObservable<IEnumerable<Tag>> TagsChanges { get; }
        [Reactive] public IEnumerable<Tag> Tags { get; set; }
        public ItemChangeset(Item? origin = null) : base(origin)
        {
            NameChanges = this.WhenAnyValue(x => x.Name);
            
            TagsChanges = this.WhenAnyValue(x => x.Tags);

            if (Origin.HasValue)
            {
                Name = (string)origin.Value.Name;
                Tags = origin.Value.Tags;
            }

            foreach (Validator<string> validator in ItemName.Validators)
            {
                this.ValidationRule(
                    vm => vm.Name,
                    name => validator.IsValid(name),
                    validator.Message)
                    .DisposeWith(disposables);
            }

        }

        public override Item Apply()
            => Apply(true);
        public Item Apply(bool dispose)
        {
            var res = new Item(this.Origin.Value, (ItemName)this.Name, this.Tags);
            if (dispose)
                this.Dispose();
            return res;
        }
        public override Item ToEntity()
            => ToEntity(true);
        public Item ToEntity(bool dispose)
        {
            var res = new Item((ItemName)this.Name, this.Tags);
            if (dispose)
                this.Dispose();
            return res;
        }
        public override IEnumerable<string> GetErrors() => throw new NotImplementedException();

        public override string ToString()
            => "Changeset: " + Name;

        public static readonly IEnumerable<Validator<ItemChangeset>> ValidatorList =
        new Validator<ItemChangeset>[][]
        {
            ItemName.Validators.Select(x=>new Validator<ItemChangeset>(changeset=>x.IsValid(changeset.Name), x.Message)).ToArray(),
        }.SelectMany(x => x);


        public override IEnumerable<Validator<ItemChangeset>> Validators => ValidatorList;
    }
}
