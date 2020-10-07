using DynamicData;
using DynamicData.Aggregation;
using DynamicData.Binding;

using ReactiveUI;

using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Security.Principal;
using System.Text;

using TableTopCrucible.Core.Helper;
using TableTopCrucible.Core.Models.Sources;
using TableTopCrucible.Data.Models.Views;
using TableTopCrucible.Domain.Library.WPF.Models;

namespace TableTopCrucible.Domain.Library.WPF.ViewModels
{
    public interface IMultiItemEditor : IDisposable
    {
        void BindSelection(IObservableList<ItemEx> selection);

    }
    public partial class MultiItemEditorViewModel : DisposableReactiveValidationObject<MultiItemEditorViewModel>, IMultiItemEditor
    {
        public MultiItemEditorViewModel(IDrivenTagEditor tagEditor)
        {
            TagEditor = tagEditor;
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
    }
}
