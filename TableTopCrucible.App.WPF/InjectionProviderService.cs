using System;
using System.Reactive.Subjects;

using TableTopCrucible.Core.Services;

namespace TableTopCrucible.App.WPF
{
    public class InjectionProviderService : IInjectionProviderService, IDisposable
    {
        private readonly BehaviorSubject<IServiceProvider> _provider = new BehaviorSubject<IServiceProvider>(null);
        public ISubject<IServiceProvider> Provider => _provider;

        internal void SetProvider(IServiceProvider provider)
        {
            this._provider.OnNext(provider);
        }

        #region IDisposable Support
        private bool _disposedValue = false;
        protected virtual void dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _provider.Dispose();
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
