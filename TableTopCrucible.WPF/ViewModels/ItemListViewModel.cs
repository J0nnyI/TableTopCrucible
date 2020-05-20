
using DynamicData;
using DynamicData.Binding;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.CodeDom;
using System.Collections.ObjectModel;
using System.ComponentModel.Design;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using TableTopCrucible.Domain.Models.Sources;
using TableTopCrucible.Domain.Services;
using TableTopCrucible.WPF.Commands;
using TableTopCrucible.WPF.Views;

namespace TableTopCrucible.WPF.ViewModels
{
    public class ItemListViewModel : ViewModelBase, IDisposable
    {
        private readonly IItemService _itemService;
        private readonly IInjectionProviderService _injectionProviderService;
        private readonly Subject<Unit> _destroy = new Subject<Unit>();
        public CreateItemCommand CreateItemCommand { get; }

        ReadOnlyObservableCollection<ItemCardViewModel> _items;
        public ReadOnlyObservableCollection<ItemCardViewModel> Items 
            => _items;


        public ItemListViewModel(
            IItemService itemService,
            IInjectionProviderService injectionProviderService,
            CreateItemCommand CreateItemCommand)
        {
            this._itemService = itemService ?? throw new NullReferenceException("got no itemService");
            this._injectionProviderService = injectionProviderService ?? throw new NullReferenceException("got no itemservice");
            this.CreateItemCommand = CreateItemCommand;

            this._injectionProviderService.Provider.Subscribe(
                (provider) =>
            {
                if (provider == null)
                    return;
                this._itemService
                .Get()
                .Connect()
                .Transform((item) =>
                {
                    var viewModel = provider.GetRequiredService<ItemCardViewModel>();
                    viewModel.ItemChanges.OnNext(item);
                    return viewModel;
                })
                .DisposeMany()
                .Bind(out _items)
                .TakeUntil(_destroy)
                .Subscribe();
            });
        }

        #region IDisposable Support
        private bool _disposedValue = false; // To detect redundant calls

        protected virtual void dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    _destroy.OnNext(Unit.Default);
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                _disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~ItemListViewModel()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
