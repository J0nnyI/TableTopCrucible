using DynamicData;
using DynamicData.Binding;
using DynamicData.ReactiveUI;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Extensions;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows.Data;
using System.Windows.Input;

using TableTopCrucible.Core.Models.Sources;
using TableTopCrucible.Core.Models.ValueTypes;
using TableTopCrucible.Core.Utilities;
using TableTopCrucible.Data.Models.Views;
using TableTopCrucible.Data.Services;
using TableTopCrucible.Domain.Models.ValueTypes;
using TableTopCrucible.WPF.Commands;

namespace TableTopCrucible.Domain.Library.WPF.ViewModels
{
    public class TagEditorViewModel : DisposableReactiveValidationObject<TagEditorViewModel>
    {
        [Reactive]
        public string NewTag { get; set; }

        [Reactive]
        public bool IsEditmode { get; set; }

        [Reactive]
        public bool IsReadOnly { get; set; }








        // AvailableTags        //all available tags
        SourceList<Tag> availableTagsChanges = new SourceList<Tag>();
        private readonly ReadOnlyObservableCollection<Tag> _availableTags;
        ObservableCollection<Tag> AvailableTags;

        // SelectedTags         //the tags of the item
        SourceList<Tag> selectedTagsChanges = new SourceList<Tag>();



        // Marked Tags          //marked for delete
        SourceList<Tag> markedTagsChanges = new SourceList<Tag>();




        public TagEditorViewModel()
        {
            availableTagsChanges.Connect().Bind(out _availableTags);
        }
    }
}
