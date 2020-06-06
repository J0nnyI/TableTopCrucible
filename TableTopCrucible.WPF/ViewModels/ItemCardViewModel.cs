using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Extensions;

using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows.Input;

using TableTopCrucible.Domain.Models;
using TableTopCrucible.Domain.Models.Sources;
using TableTopCrucible.Domain.Models.ValueTypes;
using TableTopCrucible.WPF.Commands;

namespace TableTopCrucible.WPF.ViewModels
{
    public class ItemCardViewModel : DisposableReactiveValidationObject<ItemCardViewModel>
    {

        #region observable properties

        public BehaviorSubject<string> ItemNameChanges = new BehaviorSubject<string>(null);
        private readonly ObservableAsPropertyHelper<string> _itemName;
        public string ItemName
        {
            get => _itemName.Value;
            set => ItemNameChanges.OnNext(value);
        }
        [Reactive] public ItemChangeset Changeset { get; private set; }

        public SubjectBase<Item?> ItemChanges { get; } = new BehaviorSubject<Item?>(null);
        private readonly ObservableAsPropertyHelper<Item?> _item;
        public Item? Item
            => _item.Value;

        public BehaviorSubject<bool> EditModeChanges { get; } = new BehaviorSubject<bool>(false);
        private readonly ObservableAsPropertyHelper<bool> _editMode;
        public bool EditMode
        {
            get => _editMode.Value;
            set => this.EditModeChanges.OnNext(value);
        }

        private readonly BehaviorSubject<bool> _editableChanges = new BehaviorSubject<bool>(true);
        private readonly ObservableAsPropertyHelper<bool> _editable;
        public bool Editable
        {
            get => _editable.Value;
            set => _editableChanges.OnNext(value);
        }

        [Reactive]
        public bool IsExpanded { get; set; }

        #endregion

        public TagEditorViewModel TagEditor { get; }
        #region Commands
        public ICommand DeleteItemCommand { get; }
        public ICommand SaveItemCommand { get; }
        public ICommand EnterEditmode { get; }
        public ICommand UndoCommand { get; }
        public ICommand ToggleExpansionCommand { get; }

        #endregion
        public ItemCardViewModel(
            TagEditorViewModel tagEditorViewModel,

            DeleteItemCommand deleteItemCommand,
            SaveItemCommand saveItemCommand
            )
        {
            this.TagEditor = tagEditorViewModel;


            #region Observables 


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
            this._itemName =
                this.ItemNameChanges
                .TakeUntil(destroy)
                .ToProperty(this, nameof(ItemName));


            this.ItemChanges
                .TakeUntil(destroy)
                .Subscribe(item =>
                {
                    this.ItemName = (string)item?.Name;
                    this.TagEditor.Tags = item?.Tags;
                });
            this.ItemNameChanges
                .TakeUntil(destroy)
                .Subscribe(name => { if (this.Changeset != null) { this.Changeset.Name = name; } });
            this.TagEditor.TagsChanges
                .TakeUntil(destroy)
                .Subscribe(tags => { if (this.Changeset != null) { this.Changeset.Tags = tags; } });
            this.EditModeChanges
                .TakeUntil(destroy)
                .Subscribe(editMode =>
                {
                    if (editMode)
                        this.Changeset = new ItemChangeset(this.Item);
                    else
                        this.Changeset = null;
                    this.TagEditor.IsEditmode = this.EditMode;
                    this.ItemName = (string)Item?.Name;

                });
            #endregion
            #region commands
            // DI
            this.DeleteItemCommand = deleteItemCommand;
            this.SaveItemCommand = saveItemCommand;
            saveItemCommand.ItemSaved += this.SaveItemCommand_ItemSaved;
            // Relays
            this.EnterEditmode = new RelayCommand(_ => this.EditMode = true, _ => this.Editable);
            this.UndoCommand = new RelayCommand(_ => this._undo());
            this.ToggleExpansionCommand = new RelayCommand(_ => this.IsExpanded = !this.IsExpanded);
            #endregion


            #region validators

            foreach (Validator<string> validator in Domain.Models.ValueTypes.ItemName.Validators)
            {
                this.ValidationRule(
                    vm => vm.ItemName,
                    name => validator.IsValid(name),
                    validator.Message)
                    .DisposeWith(disposables);
            }
            #endregion
        }

        private void SaveItemCommand_ItemSaved(object sender, ItemSavedEventArgs e)
        {
            if (e.Item.Id == this.Item?.Id && !this.IsDisposed)
                this.EditMode = false;
        }
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

            base.OnDispose();
        }
    }
}
