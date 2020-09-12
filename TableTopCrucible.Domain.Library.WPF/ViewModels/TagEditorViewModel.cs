using DynamicData;
using DynamicData.Binding;

using FluentValidation;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Extensions;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

using TableTopCrucible.Core.Helper;
using TableTopCrucible.Core.Models.Sources;
using TableTopCrucible.Data.Services;
using TableTopCrucible.Domain.Models.ValueTypes;
using TableTopCrucible.WPF.Commands;

namespace TableTopCrucible.Domain.Library.WPF.ViewModels
{
    public interface ITagEditor
    {
        bool Editmode { get; set; }
        bool PermitNewTags { get; set; }
        bool CompletePool { get; set; }

        ISourceList<Tag> Selection { get; }
        IObservableList<Tag> Tagpool { get; }

        void SetTagpool(IObservableList<Tag> tagpool);
        void SetSelection(IEnumerable<Tag> tags);
        void Select(Tag tag);
    }
    public class TagBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isPrimary)
            {
                return isPrimary ? Brushes.Black : Brushes.DarkGray;
            }
            throw new InvalidOperationException("tagEx expected");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public struct TagEx : IComparable
    {
        public TagEx(Tag tag, bool isPrimary)
        {
            Tag = tag;
            IsPrimary = isPrimary;
        }

        public Tag Tag { get; }
        public bool IsPrimary { get; }

        public int CompareTo(object obj)
        {
            if (obj is TagEx tag)
            {
                if (this.IsPrimary && !tag.IsPrimary)
                    return -1;
                if (!this.IsPrimary && tag.IsPrimary)
                    return 1;
                return Tag.CompareTo(tag.Tag);
            }
            throw new InvalidOperationException("must compare to TagEx");
        }
        public override string ToString() => Tag.ToString();
    }
    public class TagEditorViewModel : DisposableReactiveValidationObject<TagEditorViewModel>, ITagEditor
    {
        private readonly IItemService itemService;

        public TagEditorViewModel(IItemService itemService)
        {
            this.itemService = itemService;
            this.Selection
                .Connect()
                .Bind(SelectionBinding)
                .TakeUntil(destroy)
                .Subscribe();
            this.NewTagChanges =
                this.WhenAnyValue(vm => vm.NewTag)
                .TakeUntil(destroy);

            this.AddTag = new RelayCommand(e =>
            {
                if (e is KeyEventArgs args && args.Key != Key.Enter)
                    return;
                this.Select((Tag)NewTag);
                this.NewTag = string.Empty;
            }, _ => !this.HasErrors);
            this.RemoveTags = new RelayCommand(_ => this.Deselect(this.MarkedTags), _ => this.MarkedTags.Any());
            this.MarkingChanged = new RelayCommand(
                par =>
                {
                    if (par is IEnumerable lst)
                        this.MarkedTags = lst.OfType<Tag>().ToArray();
                }
            );

            this.allTags = this.itemService.GetTags().Connect();

            this.CompletePoolChanges = this.WhenAnyValue(vm => vm.CompletePool).TakeUntil(destroy);

            this.ValidationRule(
                vm => vm.NewTag,
                newTag => !Tag.Validate(newTag).Any() && (PermitNewTags || this.Tagpool.Items.Contains((Tag)newTag)) && !this.Selection.Items.Contains((Tag)newTag),
                newTag =>
                {
                    if (!Tag.Validate(newTag).Any())
                    {
                        if (!PermitNewTags && !this.Tagpool.Items.Contains((Tag)newTag))
                            return "there is no item with this tag";
                        if (this.Selection.Items.Contains((Tag)newTag))
                            return "this tag has already been selected";

                    }
                    return string.Join(Environment.NewLine, Tag.Validate(newTag));
                }
            );
        }
        public ICommand RemoveTags { get; }
        public ICommand AddTag { get; }
        public ICommand MarkingChanged { get; }
        private IEnumerable<Tag> MarkedTags { get; set; } = new Tag[0];
        [Reactive]
        public string NewTag { get; set; } = string.Empty;
        [Reactive]
        public bool CompletePool { get; set; }
        public IObservable<bool> CompletePoolChanges { get; set; }
        IObservable<string> NewTagChanges { get; }
        [Reactive]
        public bool Editmode { get; set; }
        [Reactive]
        public bool PermitNewTags { get; set; }
        public ISourceList<Tag> Selection { get; } = new SourceList<Tag>();
        [Reactive]
        public IObservableList<Tag> Tagpool { get; private set; }
        public ObservableCollectionExtended<TagEx> TagpoolBinding { get; } = new ObservableCollectionExtended<TagEx>();
        public ObservableCollectionExtended<Tag> SelectionBinding { get; } = new ObservableCollectionExtended<Tag>();
        private readonly Subject<Unit> tagpoolReset = new Subject<Unit>();
        private readonly IObservable<IChangeSet<Tag>> allTags;
        public void SetTagpool(IObservableList<Tag> tagpool)
        {
            this.tagpoolReset.OnNext(new Unit());
            this.Tagpool = tagpool;



            this.Tagpool
                .Connect()
                .Filter(
                    NewTagChanges.Select<string, Func<Tag, bool>>(
                        newTag => tag => ((string)tag).ToLower().Contains(newTag.ToLower())
                    )
                )
                .Except(Selection.Connect())
                .Transform(tag => new TagEx(tag, true))
                .Merge(
                    this.CompletePoolChanges
                        .DistinctUntilChanged()
                        .Select(completePool =>
                            completePool
                                ? this.allTags
                                    .Except(
                                        this.Tagpool.Connect().Merge(
                                        this.Selection.Connect()))
                                    .Transform(tag => new TagEx(tag, false))
                                : Observable.Empty<IChangeSet<TagEx>>()
                        )
                        .Switch()
                    )
                .Sort()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(TagpoolBinding)
                .TakeUntil(tagpoolReset)
                .TakeUntil(destroy)
                .Subscribe();
        }
        public void SetSelection(IEnumerable<Tag> tags)
        {
            this.Selection.Clear();
            this.Selection.AddRange(tags);
        }
        public void Select(Tag tag)
        {
            if (!PermitNewTags && !Tagpool.Items.Contains(tag))
                return;
            this.Selection.Add(tag);
        }
        public void Select(string tag)
        {
            if (this.HasErrors)
                return;
            this.Select((Tag)tag);
        }
        public void Deselect(IEnumerable<Tag> tags)
        {
            this.Selection.RemoveMany(tags);
        }
    }
}
