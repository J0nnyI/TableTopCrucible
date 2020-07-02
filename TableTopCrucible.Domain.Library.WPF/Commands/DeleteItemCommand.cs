using DynamicData;

using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows.Input;

using TableTopCrucible.Data.Services;
using TableTopCrucible.Domain.Models.ValueTypes.IDs;

namespace TableTopCrucible.WPF.Commands
{
    public class DeleteItemCommand : ICommand, IDisposable
    {
        public event EventHandler CanExecuteChanged;
        private SubjectBase<Unit> destroy = new Subject<Unit>();

        public bool CanExecute(object parameter)
        {
            if (parameter is ItemId id)
                return this._itemService.CanDelete(id);
            else
                return false;
        }
        public void Execute(object parameter)
        {
            if (parameter is ItemId itemId)
            {
                this._itemService.Delete(itemId);
            }

        }

        private readonly IItemService _itemService;
        public DeleteItemCommand(IItemService itemService)
        {
            this._itemService = itemService;
            this._itemService
                .Get()
                .Connect()
                .Transform(x => x.Id)
                .DistinctUntilChanged()
                .TakeUntil(destroy)
                .Subscribe(
                _ => CanExecuteChanged?.Invoke(this, new EventArgs()));
            CommandManager.RequerySuggested += this.CommandManager_RequerySuggested;
        }

        private void CommandManager_RequerySuggested(object sender, EventArgs e)
        {
            this.CanExecuteChanged?.Invoke(this, e);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    this.destroy.OnNext(Unit.Default);
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~DeleteItemCommand()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
