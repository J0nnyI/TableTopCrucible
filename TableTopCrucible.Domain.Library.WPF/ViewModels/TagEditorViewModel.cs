using DynamicData;
using DynamicData.Binding;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Input;

using TableTopCrucible.Core.Models.Sources;
using TableTopCrucible.Domain.Models.ValueTypes;
using TableTopCrucible.WPF.Commands;

namespace TableTopCrucible.Domain.Library.WPF.ViewModels
{
    public interface ITagEditor
    {
        bool Editmode { get; set; }
        bool PermitNewTags { get; set; }

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

            this.AddTag = new RelayCommand(_ => this.Select((Tag)NewTag), _ => Tag.Validate(NewTag).Any());
            this.NewTagChanges =
                this.WhenAnyValue(vm => vm.NewTag)
                .TakeUntil(destroy);
        }


        public ICommand RemoveTags { get; }
        public ICommand AddTag { get; }
        [Reactive]
        public string NewTag { get; set; } = string.Empty;
        IObservable<string> NewTagChanges { get; }
        [Reactive]
        public bool Editmode { get; set; }
        [Reactive]
        public bool PermitNewTags { get; set; }
        public ISourceList<Tag> Selection { get; } = new SourceList<Tag>();
        [Reactive]
        public IObservableList<Tag> Tagpool { get; private set; }
        public ObservableCollectionExtended<Tag> TagpoolBinding { get; } = new ObservableCollectionExtended<Tag>();
        public ObservableCollectionExtended<Tag> SelectionBinding { get; } = new ObservableCollectionExtended<Tag>();
        public void SetTagpool(IObservableList<Tag> tagpool)
        {
            this.Tagpool = tagpool;
            this.Tagpool
                .Connect()
                .Except(Selection.Connect())
                .Filter(
                    NewTagChanges.Select<string, Func<Tag, bool>>(
                        newTag => tag => ((string)tag).Contains((string)newTag)
                    )
                )
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
