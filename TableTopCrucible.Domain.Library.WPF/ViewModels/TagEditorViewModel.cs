using DynamicData;
using DynamicData.Binding;

using FluentValidation;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Extensions;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

using TableTopCrucible.Core.Helper;
using TableTopCrucible.Core.Models.Sources;
using TableTopCrucible.Data.Models.ValueTypes;
using TableTopCrucible.Data.Services;
using TableTopCrucible.Domain.Library.WPF.Models;
using TableTopCrucible.WPF.Commands;

namespace TableTopCrucible.Domain.Library.WPF.ViewModels
{
    public class TagBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isPrimary)
            {
                return isPrimary ? Brushes.Black : Brushes.DarkGray;
            }
            throw new InvalidOperationException("tagEx expected");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    public interface IManualTagEditor : ITagEditor
    {
        void SetSelection(IEnumerable<Tag> tags);

    }

    public class TagEditorViewModel : TagEditorViewModelBase, IManualTagEditor
    {
        protected readonly ISourceList<Tag> selection = new SourceList<Tag>();
        public override IObservableList<Tag> Selection => selection;
        public ObservableCollectionExtended<Tag> SelectionBinding { get; } = new ObservableCollectionExtended<Tag>();

        public TagEditorViewModel(IItemService itemService) : base(itemService)
        {
            this.OnSelectionUpdate();
            this.Selection
                .Connect()
                .Bind(SelectionBinding)
                .TakeUntil(destroy)
                .Subscribe();
        }
        public void SetSelection(IEnumerable<Tag> tags)
        {
            this.selection.Edit(eList =>
            {
                eList.Clear();
                eList.AddRange(tags);
            });
        }
        public override void Select(Tag tag)
        {
            if (!PermitNewTags && !Tagpool.Items.Contains(tag))
                return;
            this.selection.Add(tag);
        }
        public void Select(string tag)
        {
            if (this.HasErrors)
                return;
            this.Select((Tag)tag);
        }
        public override void Deselect(IEnumerable<Tag> tags)
        {
            this.selection.RemoveMany(tags);
        }
    }
}
