using DynamicData;
using DynamicData.Binding;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Extensions;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows;
using System.Windows.Input;

using TableTopCrucible.Core.Helper;
using TableTopCrucible.Core.Models.Sources;
using TableTopCrucible.Data.Models.ValueTypes;
using TableTopCrucible.Data.Services;
using TableTopCrucible.WPF.Commands;

namespace TableTopCrucible.Domain.Library.WPF.ViewModels
{
    public abstract class TagEditorViewModelBase : DisposableReactiveValidationObject<TagEditorViewModel>, ITagEditor
    {
        private readonly IItemService itemService;
        public abstract IObservableList<Tag> Selection { get; }
        public TagEditorViewModelBase(IItemService itemService)
        {
            this.itemService = itemService;
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

        }
        protected virtual void OnSelectionUpdate()
        {
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
        [Reactive]
        public IObservableList<Tag> Tagpool { get; private set; }
        public ObservableCollectionExtended<TagEx> TagpoolBinding { get; } = new ObservableCollectionExtended<TagEx>();
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
                .Subscribe(_ => { }, ex =>
                {
                    MessageBox.Show($"tag-editor pool exception: {nameof(TagEditorViewModel)}: tagpool"+Environment.NewLine+ex.ToString());
                });
        }

        public abstract void Select(Tag tag);
        public abstract void Deselect(IEnumerable<Tag> tags);
    }
}
