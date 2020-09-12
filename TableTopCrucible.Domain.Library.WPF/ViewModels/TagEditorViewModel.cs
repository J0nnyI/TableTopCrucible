using DynamicData;
using DynamicData.Binding;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Linq;

using TableTopCrucible.Core.Models.Sources;
using TableTopCrucible.Domain.Models.ValueTypes;

namespace TableTopCrucible.Domain.Library.WPF.ViewModels
{
    public interface ITagEditor
    {
        bool IsEditmode { get; set; }
        bool IsReadOnly { get; set; }

        ISourceList<Tag> Selection { get; }
        IObservableList<Tag> Tagpool { get; }

        void SetTagpool(IObservableList<Tag> tagpool);
        void SetSelection(IEnumerable<Tag> tags);
        void Select(Tag tag);
    }

    public class TagEditorViewModel : DisposableReactiveValidationObject<TagEditorViewModel>, ITagEditor
    {

        public TagEditorViewModel()
        {
            this.Selection
                .Connect()
                .Bind(SelectionBinding)
                .Subscribe();
        }

        [Reactive]
        public string NewTag { get; set; }

        public bool IsEditmode { get; set; }
        public bool IsReadOnly { get; set; }
        public ISourceList<Tag> Selection { get; } = new SourceList<Tag>();
        public IObservableList<Tag> Tagpool { get; set; }
        public ObservableCollectionExtended<Tag> TagpoolBinding { get; } = new ObservableCollectionExtended<Tag>();
        public ObservableCollectionExtended<Tag> SelectionBinding { get; } = new ObservableCollectionExtended<Tag>();
        public void SetTagpool(IObservableList<Tag> tagpool)
        {
            this.Tagpool = tagpool;
            this.Tagpool
                .Connect()
                .Except(Selection.Connect())
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(TagpoolBinding)
                .Subscribe();
        }
        public void SetSelection(IEnumerable<Tag> tags)
        {
            this.Selection.Clear();
            this.Selection.AddRange(tags);
        }
        public void Select(Tag tag)
        {
            this.Selection.Add(tag);
        }
    }
}
