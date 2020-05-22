using ReactiveUI;
using ReactiveUI.Validation.Extensions;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows.Controls;
using System.Windows.Input;
using TableTopCrucible.Domain.Models;
using TableTopCrucible.Domain.Models.Sources;
using TableTopCrucible.Domain.ValueTypes;
using TableTopCrucible.WPF.Commands;

namespace TableTopCrucible.WPF.ViewModels
{
    public class ItemCardViewModel : DisposableReactiveValidationObject<ItemCardViewModel>
    {

        #region observable properties

        public SubjectBase<Item?> ItemChanges { get; } = new BehaviorSubject<Item?>(null);
        private readonly ObservableAsPropertyHelper<Item?> _item;
        public Item? Item
            => _item.Value;

        public IObservable<bool> EditModeChanges { get; }
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

        #endregion

        public TagEditorViewModel TagEditor { get; }
        #region Commands
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
            this.TagEditor = tagEditorViewModel;


            #region Observables 

            // reader
            this.EditModeChanges =
                this._changesetChanges
                .Select(x => x != null);

            // properties
            this._item =
                this.ItemChanges
                .TakeUntil(destroy)
                .ToProperty(this, nameof(Item));
            this._editable =
                this._editableChanges
                .TakeUntil(destroy)
                .ToProperty(this, nameof(Editable));
            this._editMode =
                this.EditModeChanges
                .TakeUntil(destroy)
                .ToProperty(this, nameof(EditMode));
            this._changeset =
                this._changesetChanges
                .TakeUntil(destroy)
                .ToProperty(this, nameof(Changeset));

            // subscriptions
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
            this.EditModeChanges
                .TakeUntil(destroy)
                .Subscribe(changeset => this.TagEditor.IsEditmode = this.EditMode);
            #endregion
            #region commands
            // DI
            this.DeleteItemCommand = deleteItemCommand;
            this.SaveItemCommand = saveItemCommand;
            saveItemCommand.ItemSaved += this.SaveItemCommand_ItemSaved;
            // Relays
            this.EnterEditmode = new RelayCommand(_ => this._enterEditMode(), _ => this._canEnterEditMode());
            this.UndoCommand = new RelayCommand(_ => this._undo());
            #endregion
            #region validators

            this.ValidationRule(vm => 
            {
                return vm._changesetChanges
                 .Where(x => x != null)
                 .Select(cs => cs.NameChanges)
                 .Switch()
                 .Select(itemName => ItemName.Validate(itemName).Any())
                 .TakeUntil(destroy);
            } , (vm, res) =>res?"noERror": "error");

            
            #endregion
        }

        private void SaveItemCommand_ItemSaved(object sender, ItemSavedEventArgs e)
        {
            if (e.Item.Id == this.Item?.Id && !this.IsDisposed)
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

        protected override void OnDispose()
        {
            this.ItemChanges.OnCompleted();
            this.ItemChanges.Dispose();

            this._editableChanges.OnCompleted();
            this._editableChanges.Dispose();

            this._changesetChanges.OnCompleted();
            this._changesetChanges.Dispose();

            base.OnDispose();
        }
    }
}
