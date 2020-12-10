using DynamicData;
using DynamicData.Aggregation;
using DynamicData.Binding;

using ReactiveUI;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Input;

using TableTopCrucible.Core.Models.Sources;
using TableTopCrucible.Data.Models.ValueTypes;
using TableTopCrucible.Data.Models.Views;
using TableTopCrucible.Data.Services;
using TableTopCrucible.Domain.Library.WPF.Commands;
using TableTopCrucible.Data.Models.Sources;
using TableTopCrucible.FeatureCore.WPF.Tagging.Models;
using TableTopCrucible.FeatureCore.WPF.Tagging.ViewModels;
using TableTopCrucible.WPF.Commands;

namespace TableTopCrucible.Domain.Library.WPF.ViewModels
{
    public interface IMultiItemEditor : IDisposable
    {
        void BindSelection(IObservableList<ItemEx> selection);
        void SetTagpool(IObservableList<Tag> tagpool);

    }
    public partial class MultiItemEditorViewModel : DisposableReactiveValidationObject, IMultiItemEditor
    {
        private readonly IItemDataService itemService;

        public MultiItemEditorViewModel(IDrivenTagEditor tagEditor, IItemDataService itemService, CreateThumbnailsCommand createThumbnails)
        {
            TagEditor = tagEditor;
            this.itemService = itemService;
            CreateThumbnails = createThumbnails;
            this.TagEditor.Editmode = true;
            this.TagEditor.PermitNewTags = true;
            this.TagEditor.OnSelection += TagEditor_OnSelection;
            this.TagEditor.OnDeselection += TagEditor_OnDeselection;
        }

        private void TagEditor_OnDeselection(object sender, IEnumerable<Tag> e)
        {
            this.itemService.Patch(this.Selection.Items.Select(itemEx =>
            {
                var changeset = new ItemChangeset(itemEx.SourceItem);
                changeset.Tags = changeset.Tags.Except(e);
                return changeset;
            }));
        }

        private void TagEditor_OnSelection(object sender, IEnumerable<Tag> e)
        {
            this.itemService.Patch(
                this.Selection.Items.Select(itemEx =>
                {
                    var changeset = new ItemChangeset(itemEx.SourceItem);
                    var tags = changeset.Tags.ToList();
                    tags.Add(e);
                    changeset.Tags = tags.Distinct();
                    return changeset;
                }),RxApp.TaskpoolScheduler

            );
        }

        public IObservableList<ItemEx> Selection { get; private set; }
        public ObservableCollectionExtended<CountedTag> CountedTags { get; } = new ObservableCollectionExtended<CountedTag>();
        public ObservableCollectionExtended<ItemEx> SelectionBinding { get; } = new ObservableCollectionExtended<ItemEx>();
        public IDrivenTagEditor TagEditor { get; }
        public CreateThumbnailsCommand CreateThumbnails { get; }

        public void BindSelection(IObservableList<ItemEx> selection)
        {
            if (Selection != null)
                throw new InvalidOperationException("multiItemEditor: Selection is already set");

            this.Selection = selection;
            this.Selection.Connect()
                .TakeUntil(destroy)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(SelectionBinding)
                .Subscribe(_ => { }, ex =>
                {
                    MessageBox.Show(ex.ToString(),$"{nameof(MultiItemEditorViewModel)}.{nameof(BindSelection)}: selection subscription threw exception");
                });

            var count = this.Selection.Connect().Count().ObserveOn(RxApp.MainThreadScheduler);

            var selectedTags = this.Selection.Connect()
                .TransformMany(item => item.Tags)
                .GroupOn(tag => tag)
                .Transform(group =>
                    new CountedTag(count, group.List.Connect().Count().ObserveOn(RxApp.MainThreadScheduler), group.GroupKey)
                )
                .DisposeMany()
                //.Sort()
                ;

            this.TagEditor.BindSelection(selectedTags);
        }
        public void SetTagpool(IObservableList<Tag> tagpool)
            => this.TagEditor.SetTagpool(tagpool);
    }
}
