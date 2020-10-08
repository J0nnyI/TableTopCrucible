using DynamicData;
using DynamicData.Binding;

using ReactiveUI;

using System;
using System.Collections.Generic;
using System.Reactive.Linq;

using TableTopCrucible.Data.Models.ValueTypes;
using TableTopCrucible.Data.Services;
using TableTopCrucible.Domain.Library.WPF.Models;

namespace TableTopCrucible.Domain.Library.WPF.ViewModels
{
    public interface IDrivenTagEditor : ITagEditor
    {
        event EventHandler<IEnumerable<Tag>> OnDeselection;
        event EventHandler<Tag> OnSelection;
        void BindSelection(IObservable<IChangeSet<CountedTag>> tags);

    }

    public class DrivenTagEditorViewModel : TagEditorViewModelBase, IDrivenTagEditor
    {
        public event EventHandler<IEnumerable<Tag>> OnDeselection;
        public event EventHandler<Tag> OnSelection;

        public DrivenTagEditorViewModel(IItemService itemService) : base(itemService)
        {
        }
        public ObservableCollectionExtended<CountedTag> SelectionBinding { get; } = new ObservableCollectionExtended<CountedTag>();

        private IObservableList<Tag> _selection;
        public override IObservableList<Tag> Selection => _selection;

        public void BindSelection(IObservable<IChangeSet<CountedTag>> tags)
        {
            if (this.Selection != null)
                throw new InvalidOperationException("Selection has already been set");
            tags.Bind(SelectionBinding).TakeUntil(destroy).Subscribe();
            this._selection = tags.Transform(tag => tag.Tag).AsObservableList();
            this.OnSelectionUpdate();
        }

        public override void Deselect(IEnumerable<Tag> tags)
        {
            this.OnDeselection?.Invoke(this, tags);
        }

        public override void Select(Tag tag)
        {
            this.OnSelection?.Invoke(this, tag);
        }

    }
}
