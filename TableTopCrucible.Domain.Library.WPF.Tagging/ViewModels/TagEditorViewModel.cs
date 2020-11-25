using DynamicData;
using DynamicData.Binding;

using ReactiveUI;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

using TableTopCrucible.Core.Helper;
using TableTopCrucible.Data.Models.ValueTypes;
using TableTopCrucible.Data.Services;

namespace TableTopCrucible.Domain.Library.WPF.Tagging.ViewModels
{


    public interface IManualTagEditor : ITagEditor
    {
        void SetSelection(IEnumerable<Tag> tags);

    }

    public class TagEditorViewModel : TagEditorViewModelBase, IManualTagEditor
    {
        protected readonly ISourceList<Tag> selection = new SourceList<Tag>();
        public override IObservableList<Tag> Selection => selection;
        public ObservableCollectionExtended<Tag> SelectionBinding { get; } = new ObservableCollectionExtended<Tag>();

        public TagEditorViewModel(IItemDataService itemService) : base(itemService)
        {
            OnSelectionUpdate();
            Selection
                .Connect()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(SelectionBinding)
                .TakeUntil(destroy)
                .Subscribe();
            TagpoolExceptions = Selection.Connect();
        }
        public void SetSelection(IEnumerable<Tag> tags)
        {
            if (tags == null)
                return;

            selection.Edit(eList =>
            {
                eList.Clear();
                eList.AddRange(tags);
            });
        }
        public override void Select(IEnumerable<Tag> tags)
        {
            if (!PermitNewTags)
                tags = tags.Intersect(Tagpool.Items);
            selection.AddRange(tags);
        }
        public void Select(string tag)
        {
            if (HasErrors)
                return;
            Select(((Tag)tag).AsArray());
        }
        public override void Deselect(IEnumerable<Tag> tags)
            => selection.RemoveMany(tags);
        public override string Validate(string newTag)
        {
            var basics = base.Validate(newTag);
            if (!string.IsNullOrWhiteSpace(basics))
                return basics;

            if (Selection.Items.Contains((Tag)newTag))
                return "this tag has already been selected";

            return null;
        }
        public override IObservable<IChangeSet<Tag>> TagpoolExceptions { get; }
    }
}
