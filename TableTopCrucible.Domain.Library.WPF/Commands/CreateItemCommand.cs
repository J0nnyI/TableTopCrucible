using System;
using System.Windows.Input;

using TableTopCrucible.Data.Models.Sources;
using TableTopCrucible.Data.Models.ValueTypes;
using TableTopCrucible.Data.Services;

namespace TableTopCrucible.WPF.Commands
{
    public struct ItemCreatedEventArgs
    {
        public ItemCreatedEventArgs(Item item)
        {
            this.Item = item;
        }

        public Item Item { get; }
    }

    public class CreateItemCommand : ICommand
    {
        private readonly IItemDataService _itemService;
        public CreateItemCommand(IItemDataService itemService)
        {
            this._itemService = itemService;
        }

        public event EventHandler CanExecuteChanged;
        public event EventHandler<ItemCreatedEventArgs> ItemCreated;

        public bool CanExecute(object parameter) => true;
        public void Execute(object parameter)
        {
            var item = new ItemChangeset()
            {
                Name = "new Item",
                Tags = new Tag[] { (Tag)"new" }
            };
            var entity = this._itemService.Patch(item);
            this.ItemCreated?.Invoke(this, new ItemCreatedEventArgs(entity));
        }
    }
}