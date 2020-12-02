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
using TableTopCrucible.FeatureCore.WPF.Tagging.Models;
using TableTopCrucible.WPF.Commands;

namespace TableTopCrucible.FeatureCore.WPF.Tagging.ViewModels
{
    public abstract class TagEditorViewModelBase : DisposableReactiveValidationObject, ITagEditor
    {
        private readonly IItemDataService itemService;
        public abstract IObservableList<Tag> Selection { get; }
        public abstract IObservable<IChangeSet<Tag>> TagpoolExceptions { get; }
        [Reactive]
        public int MarkedIndex { get; set; } = -1;
        public TagEditorViewModelBase(IItemDataService itemService)
        {
            this.itemService = itemService;
            NewTagChanges =
                this.WhenAnyValue(vm => vm.NewTag)
                .TakeUntil(destroy);

            AddTagButtonCommand = new RelayCommand(
                e =>
                {
                    Select(((Tag)NewTag).AsArray());
                    NewTag = string.Empty;
                },
                _ => !HasErrors);

            AddTagTextboxCommand = new RelayCommand(
                e =>
                {
                    if (e is KeyEventArgs args && args.Key != Key.Enter)
                        return;
                    Select(((Tag)NewTag).AsArray());
                    NewTag = string.Empty;
                },
                _ => !HasErrors);

            RemoveTags = new RelayCommand(_ => Deselect(markedTags), _ => markedTags.Any());
            MarkingChanged = new RelayCommand(
                par =>
                {
                    if (par is IEnumerable lst)
                    {
                        markedTags = lst.OfType<Tag>().ToArray();
                        if (!markedTags.Any())
                            markedTags = lst.OfType<CountedTag>().Select(tag => tag.Tag).ToArray();
                    }
                }
            );

            allTags = this.itemService.GetTags().Connect();

            CompletePoolChanges = this.WhenAnyValue(vm => vm.CompletePool).TakeUntil(destroy);

        }
        protected void UnmarkAll()
        {
            MarkedIndex = -1;
        }
        protected virtual void OnSelectionUpdate()
        {
            this.ValidationRule(
                vm => vm.NewTag,
                newTag => Validate(newTag) == null,
                Validate
            );
        }
        public virtual string Validate(string newTag)
        {
            if (!Tag.Validate(newTag).Any())
            {
                if (!PermitNewTags && !Tagpool.Items.Contains((Tag)newTag))
                    return "there is no item with this tag";
            }
            return string.Join(Environment.NewLine, Tag.Validate(newTag));
        }
        public ICommand RemoveTags { get; }
        public ICommand AddTagTextboxCommand { get; }
        public ICommand AddTagButtonCommand { get; protected set; }
        public ICommand MarkingChanged { get; }
        protected IEnumerable<Tag> markedTags { get; private set; } = new Tag[0];
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
            tagpoolReset.OnNext(new Unit());
            Tagpool = tagpool;



            Tagpool
                .Connect()
                .Filter(
                    NewTagChanges.Select<string, Func<Tag, bool>>(
                        newTag => tag => ((string)tag).ToLower().Contains(newTag.ToLower())
                    )
                )
                .Except(TagpoolExceptions)
                .Transform(tag => new TagEx(tag, true))
                .Merge(
                    CompletePoolChanges
                        .DistinctUntilChanged()
                        .Select(completePool =>
                            completePool
                                ? allTags
                                    .Except(
                                        TagpoolExceptions.Merge(
                                        Selection.Connect()))
                                    .Transform(tag => new TagEx(tag, false))
                                : Observable.Empty<IChangeSet<TagEx>>()
                        )
                        .Switch()
                    )
                //.Sort()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(TagpoolBinding)
                .TakeUntil(tagpoolReset)
                .TakeUntil(destroy)
                .Subscribe(_ => { }, ex =>
                {
                    MessageBox.Show($"({GetType().FullName}) tagpool exception: " + Environment.NewLine + ex.ToString());
                });
        }

        public abstract void Select(IEnumerable<Tag> tags);
        public abstract void Deselect(IEnumerable<Tag> tags);
    }
}
