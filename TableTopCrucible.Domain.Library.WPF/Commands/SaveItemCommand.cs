using System;
using System.Windows.Input;

using TableTopCrucible.Data.Services;
using TableTopCrucible.Domain.Models.Sources;

namespace TableTopCrucible.WPF.Commands
{
    public class ItemSavedEventArgs
    {
        public ItemSavedEventArgs(Item item)
        {
            this.Item = item;
        }

        public Item Item { get; }
    }

    public class SaveItemCommand : ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public event EventHandler<ItemSavedEventArgs> ItemSaved;

        private readonly IItemDataService _itemService;

        public bool CanExecute(object parameter)
        {
            if (parameter is ItemChangeset changeset)
                return _itemService.CanPatch(changeset);
            return false;
        }
        public void Execute(object parameter)
        {
            if (parameter is ItemChangeset changeset)
            {
                var res = this._itemService.Patch(changeset);
                this.ItemSaved?.Invoke(this, new ItemSavedEventArgs(res));
            }
        }




        public SaveItemCommand(IItemDataService itemService)
        {
            this._itemService = itemService;
        }
    }
}
