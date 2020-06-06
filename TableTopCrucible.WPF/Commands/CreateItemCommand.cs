using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Abstractions;
using ReactiveUI.Validation.Extensions;
using ReactiveUI.Validation.Helpers;
using System;
using System.ComponentModel.DataAnnotations;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Input;

using TableTopCrucible.Domain.Models.Sources;
using TableTopCrucible.Domain.Services;
using TableTopCrucible.Domain.Models.ValueTypes;

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
        private readonly IItemService _itemService;
        public CreateItemCommand(IItemService itemService)
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
                Thumbnail = @"https://i.etsystatic.com/19002916/r/il/617e49/1885302518/il_fullxfull.1885302518_9ovo.jpg",
                Tags = new Tag[] { (Tag)"new" }
            };
            var entity = this._itemService.Patch(item);
            this.ItemCreated?.Invoke(this, new ItemCreatedEventArgs(entity));
        }
    }
}