using DynamicData;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;

using TableTopCrucible.Core.Models.Sources;
using TableTopCrucible.Data.Models.Views;
using TableTopCrucible.Data.Services;
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


        public ItemListFilterViewModel(TagEditorViewModel tagEditor, IItemService itemService, IItemTagService tagService)
        {
            TagEditor = tagEditor;
            TagEditor.IsEditmode = true;


            this.FilterChanges =
                Observable.CombineLatest(
                    this.TagEditor.TagsChanges,
                    this.WhenAnyValue(x => x.TagFilterMode),
                    this.WhenAnyValue(x => x.NameFilterMode),
                    this.WhenAnyValue(x => x.NameFilter),
                    (a, b, c, d) => new Unit()
                )
                .TakeUntil(destroy)
                .Select(_ => new Func<ItemEx, bool>(Filter));
        }

        public bool Filter(ItemEx item)
        {
            if (TagEditor.Tags.Any())
            {
                switch (TagFilterMode)
                {
                    case TagFilterMode.HasAll:
                        if (TagEditor.Tags.Any(itemTag => !item.Tags.Contains(itemTag)))
                            return false;
                        break;
                    case TagFilterMode.HasAny:
                        if (!item.Tags.Any(itemTag => TagEditor.Tags.Contains(itemTag)))
                            return false;
                        break;
                    case TagFilterMode.HasNone:
                        if (item.Tags.Any(itemTag => TagEditor.Tags.Contains(itemTag)))
                            return false;
                        break;
                    default:
                        break;
                }

            }
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

        public TagEditorViewModel TagEditor { get; }
        public IEnumerable<TagFilterMode> TagFilterModes { get; } = new TagFilterMode[]
        {
            TagFilterMode.HasAll,
            TagFilterMode.HasAny,
            TagFilterMode.HasNone
        };
        [Reactive]
        public TagFilterMode TagFilterMode { get; set; } = TagFilterMode.HasAny;
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
    }
}
