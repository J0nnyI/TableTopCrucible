﻿using DynamicData;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Windows.Forms;
using TableTopCrucible.Core.Helper;
using TableTopCrucible.Core.Models.Sources;
using TableTopCrucible.Data.Models.Views;
using TableTopCrucible.Data.Services;
using TableTopCrucible.Domain.Models.Sources;
using TableTopCrucible.Domain.Models.ValueTypes;

namespace TableTopCrucible.Domain.Library.WPF.ViewModels
{
    public enum TagFilterMode
    {
        HasAll,
        HasAny,
        HasNone
    }
    public enum TextFilterMode
    {
        StartsWith,
        EndsWith,
        Equals,
        Contains
    }
    public class ItemListFilterViewModel : DisposableReactiveObjectBase
    {
        public IObservable<Func<ItemEx, bool>> FilterChanges;


        public ItemListFilterViewModel(
            ITagEditor tagWhitelist,
            ITagEditor tagBlacklist,
            IDirectoryDataService directoryDataService,
            IItemService itemService)
        {
            TagWhitelist = tagWhitelist;
            TagBlacklist = tagBlacklist;
            TagBlacklist.Editmode = TagWhitelist.Editmode = true;
            TagBlacklist.PermitNewTags = TagWhitelist.PermitNewTags = false;
            _directorySetups =
                directoryDataService.Get()
                .Connect()
                .RemoveKey()
                .Filter(
                    this.WhenAnyValue(vm=>vm.DirSetupFilter)
                    .Select<string,Func<DirectorySetup,bool>>(input=>dir=>((string)dir.Name).ToLower().Contains(input.ToLower())))
                .ToSortedCollection(x => x.Name)
                .ToProperty(this, nameof(DirectorySetups));

            this.FilterChanges = Observable.Merge(
                this.TagWhitelist.Selection.Connect(),
                this.TagBlacklist.Selection.Connect(),
                this.WhenAnyValue(x => x.NameFilterMode).Select(_ => new object()),
                this.WhenAnyValue(x => x.NameFilter),
                this.WhenAnyValue(x => x.DirectorySetupFilter).Select(_ => new object())
                )
            .TakeUntil(destroy)
            .Select(_ => new Func<ItemEx, bool>(Filter));

            this.Tagpool =
                itemService.GetTags(
                    itemService.GetExtended()
                        .Connect()
                        .Filter(FilterChanges)
                        .Transform(itemEx => itemEx.SourceItem)
                    );
            TagWhitelist.SetTagpool(
                Tagpool
                    .Except(tagBlacklist.Selection.Connect())
                    .AsObservableList()
                );
            TagBlacklist.SetTagpool(
                Tagpool
                    .Except(tagWhitelist.Selection.Connect())
                    .AsObservableList()
                );

        }
        public IObservable<IChangeSet<Tag>> Tagpool { get; private set; }
        public bool Filter(ItemEx item)
        {
            if (DirectorySetupFilter.HasValue && !item.DirectorySetups.Contains(DirectorySetupFilter.Value))
                return false;

            if (TagWhitelist.Selection.Items.Any(itemTag => !item.Tags.Contains(itemTag)))
                return false;
            if (item.Tags.Any(itemTag => TagBlacklist.Selection.Items.Contains(itemTag)))
                return false;


            if (!string.IsNullOrWhiteSpace(NameFilter))
            {
                switch (NameFilterMode)
                {
                    case TextFilterMode.StartsWith:
                        if (!((string)item.Name).ToLower().StartsWith(NameFilter.ToLower()))
                            return false;
                        break;
                    case TextFilterMode.EndsWith:
                        if (!((string)item.Name).ToLower().EndsWith(NameFilter.ToLower()))
                            return false;
                        break;
                    case TextFilterMode.Equals:
                        if (!((string)item.Name).ToLower().Equals(NameFilter.ToLower()))
                            return false;
                        break;
                    case TextFilterMode.Contains:
                        if (!((string)item.Name).ToLower().Contains(NameFilter.ToLower()))
                            return false;
                        break;
                    default:
                        break;
                }
            }
            return true;
        }
        [Reactive] public string DirSetupFilter { get; set; } = string.Empty;
        public ITagEditor TagWhitelist { get; }
        public ITagEditor TagBlacklist { get; }
        public IEnumerable<TextFilterMode> TextFilterModes { get; } = new TextFilterMode[]
        {
            TextFilterMode.Contains,
            TextFilterMode.StartsWith,
            TextFilterMode.EndsWith,
            TextFilterMode.Equals,
        };
        [Reactive]
        public TextFilterMode NameFilterMode { get; set; } = TextFilterMode.Contains;
        [Reactive]
        public string NameFilter { get; set; } = string.Empty;
        [Reactive]
        public DirectorySetup? DirectorySetupFilter { get; set; }

        private readonly ObservableAsPropertyHelper<IReadOnlyCollection<DirectorySetup>> _directorySetups;
        public IEnumerable<DirectorySetup> DirectorySetups => _directorySetups?.Value;
    }
}
