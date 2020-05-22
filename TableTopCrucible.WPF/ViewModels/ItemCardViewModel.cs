﻿using ReactiveUI;

using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows.Input;

using TableTopCrucible.Domain.Models.Sources;
using TableTopCrucible.WPF.Commands;

namespace TableTopCrucible.WPF.ViewModels
{
    public class ItemCardViewModel : DisposableReactiveValidationObject<ItemCardViewModel>
    {
        public SubjectBase<Item?> ItemChanges { get; } = new BehaviorSubject<Item?>(null);
        private readonly ObservableAsPropertyHelper<Item?> _item;
        public Item? Item
            => _item.Value;


        private readonly ObservableAsPropertyHelper<bool> _editMode;
        public bool EditMode
        {
            get => _editMode.Value;
        }

        private readonly BehaviorSubject<bool> _editableChanges = new BehaviorSubject<bool>(true);
        private readonly ObservableAsPropertyHelper<bool> _editable;
        public bool Editable
        {
            get => _editable.Value;
            set => _editableChanges.OnNext(value);
        }

        private readonly BehaviorSubject<ItemChangeset> _changesetChanges = new BehaviorSubject<ItemChangeset>(null);
        private readonly ObservableAsPropertyHelper<ItemChangeset> _changeset;
        public ItemChangeset Changeset
        {
            get => _changeset.Value;
            set => _changesetChanges.OnNext(value);
        }

        public TagEditorViewModel TagEditor { get; }
        #region todo props
        public ICommand DeleteItemCommand { get; }
        public ICommand SaveItemCommand { get; }
        public ICommand EnterEditmode { get; }
        public ICommand UndoCommand { get; }

        #endregion

        public ItemCardViewModel(
            TagEditorViewModel tagEditorViewModel,

            DeleteItemCommand deleteItemCommand,
            SaveItemCommand saveItemCommand
            )
        {
            // viewModels
            this.TagEditor = tagEditorViewModel;
            // commands
            this.DeleteItemCommand = deleteItemCommand;
            this.SaveItemCommand = saveItemCommand;
            saveItemCommand.ItemSaved += this.SaveItemCommand_ItemSaved;

            // properties
            this._item =
                ItemChanges
                .TakeUntil(destroy)
                .ToProperty(this, nameof(Item));
            this._editable =
                this._editableChanges
                .TakeUntil(destroy)
                .ToProperty(this, nameof(Editable));
            this._editMode =
                this._changesetChanges
                .Select(x => x != null)
                .TakeUntil(destroy)
                .ToProperty(this, nameof(EditMode));
            this._changeset =
                this._changesetChanges
                .TakeUntil(destroy)
                .ToProperty(this, nameof(Changeset));
            this.TagEditor.TagsChanges
                .TakeUntil(destroy)
                .Subscribe(tags =>
                {
                    if (Changeset != null)
                        Changeset.Tags = tags;
                });

            this.ItemChanges
                .TakeUntil(destroy)
                .Subscribe(item => this.TagEditor.Tags = item?.Tags);
            this._changesetChanges
                .TakeUntil(destroy)
                .Subscribe(changeset => this.TagEditor.IsEditmode = Changeset == null);
            // relay commands
            this.EnterEditmode = new RelayCommand(_ => this._enterEditMode(), _ => this._canEnterEditMode());
            this.UndoCommand = new RelayCommand(_ => this._undo());

        }

        private void SaveItemCommand_ItemSaved(object sender, ItemSavedEventArgs e)
        {
            if (e.Item.Id == this.Item?.Id)
                this.Changeset = null;
        }
        private void _enterEditMode()
            => this.Changeset = new ItemChangeset(Item);
        private bool _canEnterEditMode()
            => this.Editable;
        private void _undo()
        {
            this.Changeset = null;
        }


    }
}
