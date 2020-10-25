﻿using DynamicData;
using DynamicData.Binding;

using FluentValidation;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Extensions;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows;
using System.Windows.Input;

using TableTopCrucible.Core.Helper;
using TableTopCrucible.Core.Models.Sources;
using TableTopCrucible.Data.Models.ValueTypes;
using TableTopCrucible.Data.Services;
using TableTopCrucible.Domain.Library.WPF.Models;
using TableTopCrucible.Domain.Library.WPF.Views;

namespace TableTopCrucible.Domain.Library.WPF.ViewModels
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

        public TagEditorViewModel(IItemService itemService) : base(itemService)
        {
            this.OnSelectionUpdate();
            this.Selection
                .Connect()
                .Bind(SelectionBinding)
                .TakeUntil(destroy)
                .Subscribe();
            this.TagpoolExceptions = Selection.Connect();
        }
        public void SetSelection(IEnumerable<Tag> tags)
        {
            if (tags == null)
                return;

            this.selection.Edit(eList =>
            {
                eList.Clear();
                eList.AddRange(tags);
            });
        }
        public override void Select(IEnumerable<Tag> tags)
        {
            if (!PermitNewTags)
                tags = tags.Intersect(Tagpool.Items);
            this.selection.AddRange(tags);
        }
        public void Select(string tag)
        {
            if (this.HasErrors)
                return;
            this.Select(((Tag)tag).AsArray());
        }
        public override void Deselect(IEnumerable<Tag> tags)
            => this.selection.RemoveMany(tags);
        public override string Validate(string newTag)
        {
            var basics = base.Validate(newTag);
            if (!string.IsNullOrWhiteSpace(basics))
                return basics;

            if (this.Selection.Items.Contains((Tag)newTag))
                return "this tag has already been selected";

            return null;
        }
        public override IObservable<IChangeSet<Tag>> TagpoolExceptions { get; }
    }
}