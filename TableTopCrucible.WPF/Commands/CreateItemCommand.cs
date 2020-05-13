using System;
using System.Windows.Input;

using TableTopCrucible.Domain.Models.Sources;
using TableTopCrucible.Domain.Services;
using TableTopCrucible.Domain.ValueTypes;

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
        private IItemService itemService;
        public CreateItemCommand(IItemService itemService)
        {
            this.itemService = itemService;
        }

        public event EventHandler CanExecuteChanged;
        public event EventHandler<ItemCreatedEventArgs> ItemCreated;

        public bool CanExecute(object parameter) => true;
        public void Execute(object parameter)
        {
            var item = new ItemChangeset()
            {
                Name = (ItemName)"new Item",
                Thumbnail = (Thumbnail)@"D:\__MANAGED_FILES__\DnD\__Thumbnails__\20200126_191331.jpg",
                Tags = new Tag[] { (Tag)"new" }
            };
            var entity = this.itemService.Patch(item);
            this.ItemCreated?.Invoke(this, new ItemCreatedEventArgs(entity));
        }
    }
}
