using System;
using System.Reactive.Subjects;

using TableTopCrucible.Core.Services;

namespace TableTopCrucible.App.WPF
{
    public class InjectionProviderService : IInjectionProviderService, IDisposable
    {
        private readonly BehaviorSubject<IServiceProvider> _providerChanges = new BehaviorSubject<IServiceProvider>(null);
        public IObservable<IServiceProvider> ProviderChanges => _providerChanges;
        public IServiceProvider Provider => _providerChanges.Value;

        internal void SetProvider(IServiceProvider provider)
        {
            this._providerChanges.OnNext(provider);
        }

        #region IDisposable Support
        private bool _disposedValue = false;
        protected virtual void dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _providerChanges.Dispose();
                }
                _disposedValue = true;
            }
        }
        public void Dispose()
        {
            dispose(true);
        }
        #endregion
    }
}
