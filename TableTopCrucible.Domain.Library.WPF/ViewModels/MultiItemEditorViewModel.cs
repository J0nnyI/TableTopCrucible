using DynamicData;
using DynamicData.Aggregation;
using DynamicData.Binding;

using ReactiveUI;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Security.Principal;
using System.Text;

using TableTopCrucible.Core.Helper;
using TableTopCrucible.Core.Models.Sources;
using TableTopCrucible.Data.Models.ValueTypes;
using TableTopCrucible.Data.Models.Views;
using TableTopCrucible.Data.Services;
using TableTopCrucible.Domain.Library.WPF.Models;
using TableTopCrucible.Domain.Models.Sources;

namespace TableTopCrucible.Domain.Library.WPF.ViewModels
{
    public interface IMultiItemEditor : IDisposable
    {
        void BindSelection(IObservableList<ItemEx> selection);
        void SetTagpool(IObservableList<Tag> tagpool);

    }
    public partial class MultiItemEditorViewModel : DisposableReactiveValidationObject<MultiItemEditorViewModel>, IMultiItemEditor
    {
        private readonly IItemService itemService;

        public MultiItemEditorViewModel(IDrivenTagEditor tagEditor, IItemService itemService)
        {
            TagEditor = tagEditor;
            this.itemService = itemService;
            this.TagEditor.Editmode = true;
            this.TagEditor.PermitNewTags = true;
            this.TagEditor.OnSelection += TagEditor_OnSelection;
            this.TagEditor.OnDeselection += TagEditor_OnDeselection;
        }

        private void TagEditor_OnDeselection(object sender, IEnumerable<Tag> e)
        {
            this.itemService.Patch(this.Selection.Items.Select(itemEx => {
                var changeset = new ItemChangeset(itemEx.SourceItem);
                changeset.Tags = changeset.Tags.Except(e);
                return changeset;
            }));
        }

        private void TagEditor_OnSelection(object sender, Tag e)
        {
            this.itemService.Patch(this.Selection.Items.Select(itemEx => {
                var changeset = new ItemChangeset(itemEx.SourceItem);
                var tags = changeset.Tags.ToList();
                tags.Add(e);
                changeset.Tags = tags;
                return changeset;
            }));
        }

        public IObservableList<ItemEx> Selection { get; private set; }
        public ObservableCollectionExtended<CountedTag> CountedTags { get; } = new ObservableCollectionExtended<CountedTag>();
        public ObservableCollectionExtended<ItemEx> SelectionBinding { get; } = new ObservableCollectionExtended<ItemEx>();
        public IDrivenTagEditor TagEditor { get; }

        public void BindSelection(IObservableList<ItemEx> selection)
        {
            if (Selection != null)
                throw new InvalidOperationException("multiItemEditor: Selection is already set");

            this.Selection = selection;
            this.Selection.Connect()
                .TakeUntil(destroy)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(SelectionBinding)
                .Subscribe();

            var count = this.Selection.Connect().Count();

            var selectedTags = this.Selection.Connect()
                .TransformMany(item => item.Tags)
                .GroupOn(tag => tag)
                .Transform(group =>
                    new CountedTag(count, group.List.Connect().Count(), group.GroupKey)
                )
                .DisposeMany();

            this.TagEditor.BindSelection(selectedTags);
        }
        public void SetTagpool(IObservableList<Tag> tagpool)
            => this.TagEditor.SetTagpool(tagpool);
    }
}
