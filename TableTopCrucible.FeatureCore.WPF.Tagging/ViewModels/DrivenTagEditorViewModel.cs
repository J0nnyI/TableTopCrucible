using DynamicData;
using DynamicData.Binding;

using ReactiveUI;

using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Linq;

using TableTopCrucible.Data.Models.ValueTypes;
using TableTopCrucible.Data.Services;
using TableTopCrucible.WPF.Commands;
using TableTopCrucible.Core.Helper;
using TableTopCrucible.FeatureCore.WPF.Tagging.Models;

namespace TableTopCrucible.FeatureCore.WPF.Tagging.ViewModels
{
    public interface IDrivenTagEditor : ITagEditor
    {
        event EventHandler<IEnumerable<Tag>> OnDeselection;
        event EventHandler<IEnumerable<Tag>> OnSelection;
        void BindSelection(IObservable<IChangeSet<CountedTag>> tags);

    }

    public class DrivenTagEditorViewModel : TagEditorViewModelBase, IDrivenTagEditor
    {
        public event EventHandler<IEnumerable<Tag>> OnDeselection;
        public event EventHandler<IEnumerable<Tag>> OnSelection;

        public DrivenTagEditorViewModel(IItemDataService itemService) : base(itemService)
        {
            AddTagButtonCommand = new RelayCommand(
                e =>
                {
                    var tags = markedTags.ToList();
                    if (!HasErrors)
                        tags.Add((Tag)NewTag);

                    Select(tags);
                    NewTag = string.Empty;
                    UnmarkAll();
                },
                _ => !HasErrors || markedTags.Any());
        }
        public ObservableCollectionExtended<CountedTag> SelectionBinding { get; } = new ObservableCollectionExtended<CountedTag>();

        private IObservableList<Tag> _selection;
        public override IObservableList<Tag> Selection => _selection;

        public void BindSelection(IObservable<IChangeSet<CountedTag>> tags)
        {
            if (Selection != null)
                throw new InvalidOperationException("Selection has already been set");

            tags.ObserveOn(RxApp.MainThreadScheduler)
                .Bind(SelectionBinding)
                .TakeUntil(destroy)
                .Subscribe();

            _selection = tags.Transform(tag => tag.Tag).AsObservableList();
            _tagpoolExceptions =
                tags
                .Filter(cTag => cTag.Total == cTag.Count)
                .Transform(cTag => cTag.Tag)
                .TakeUntil(destroy);
            OnSelectionUpdate();
        }
        public override void Deselect(IEnumerable<Tag> tags)
            => OnDeselection?.Invoke(this, tags);

        public override void Select(IEnumerable<Tag> tags)
            => OnSelection?.Invoke(this, tags);
        public override string Validate(string newTag)
        {

            var basics = base.Validate(newTag);
            if (!string.IsNullOrWhiteSpace(basics))
                return basics;

            if (SelectionBinding.Any(cTag => cTag.Tag == (Tag)newTag && cTag.Total == cTag.Count))
                return "this tag has already been selected for each item";
            return null;
        }
        private IObservable<IChangeSet<Tag>> _tagpoolExceptions;
        public override IObservable<IChangeSet<Tag>> TagpoolExceptions => _tagpoolExceptions;
    }
}
