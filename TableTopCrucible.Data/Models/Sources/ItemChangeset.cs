﻿
using ReactiveUI;
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
        public BehaviorSubject<string> NameChanges { get; } = new BehaviorSubject<string>(null);
        private readonly ObservableAsPropertyHelper<string> _name;
        public string Name
        {
            get => _name.Value;
            set => NameChanges.OnNext(value);
        }

        public BehaviorSubject<IEnumerable<Tag>> TagsChanges = new BehaviorSubject<IEnumerable<Tag>>(null);
        private readonly ObservableAsPropertyHelper<IEnumerable<Tag>> _tags;
        public IEnumerable<Tag> Tags
        {
            get => _tags.Value;
            set => TagsChanges.OnNext(value);
        }
        public BehaviorSubject<string> ThumbnailChanges = new BehaviorSubject<string>(null);
        private readonly ObservableAsPropertyHelper<string> _thumbnail;
        public string Thumbnail
        {
            get => _thumbnail.Value;
            set => ThumbnailChanges.OnNext(value);
        }
        public BehaviorSubject<FileInfoHashKey?> FileChanges { get; } = new BehaviorSubject<FileInfoHashKey?>(null);
        private readonly ObservableAsPropertyHelper<FileInfoHashKey?> _file;
        public FileInfoHashKey? File
        {
            get => _file.Value;
            set => FileChanges.OnNext(value);
        }
        public ItemChangeset(Item? origin = null) : base(origin)
        {
            _name = NameChanges
                .ToProperty(this, nameof(Name));
            _thumbnail = ThumbnailChanges
                .ToProperty(this, nameof(Thumbnail));
            _tags = TagsChanges
                .ToProperty(this, nameof(Tags));
            _file = FileChanges
                .ToProperty(this, nameof(File));

            this.disposables.Add(NameChanges, ThumbnailChanges, TagsChanges, FileChanges);

            if (Origin.HasValue)
            {
                Name = (string)origin.Value.Name;
                Thumbnail = (string)origin.Value.Thumbnail;
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
            var res = new Item(this.Origin.Value, (ItemName)this.Name, this.Tags, this.File, (Thumbnail?)this.Thumbnail);
            if (dispose)
                this.Dispose();
            return res;
        }
        public override Item ToEntity()
            => ToEntity(true);
        public Item ToEntity(bool dispose)
        {
            var res = new Item((ItemName)this.Name, this.Tags, this.File, string.IsNullOrWhiteSpace(this.Thumbnail) ? null as Thumbnail? : (Thumbnail)this.Thumbnail);
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
            Models.ValueTypes.Thumbnail.Validators.Select(x=>new Validator<ItemChangeset>(changeset=>x.IsValid(changeset.Thumbnail), x.Message)).ToArray(),
        }.SelectMany(x => x);


        public override IEnumerable<Validator<ItemChangeset>> Validators => ValidatorList;
    }
}