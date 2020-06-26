using System;
using System.Windows.Input;

using TableTopCrucible.Domain.Services;

namespace TableTopCrucible.WPF.Commands
{
    public class GenerateItemsFromFilesCommand : ICommand
    {
        private readonly IItemService _itemService;

        public GenerateItemsFromFilesCommand(IItemService itemService)
        {
            this._itemService = itemService ?? throw new ArgumentNullException(nameof(itemService));
        }


        //todo^: notify changes when there are files withour item
        public event EventHandler CanExecuteChanged;

        //todo
        public bool CanExecute(object parameter) => true;
        public void Execute(object parameter)
        {
            this._itemService.AutoGenerateItems();
        }
    }
}
