using ReactiveUI.Validation.Helpers;

using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Subjects;

namespace TableTopCrucible.WPF.ViewModels
{
    public class DisposableReactiveValidationObject<T> : ReactiveValidationObject<T>, IDisposable
    {
        private readonly Subject<Unit> _destroy = new Subject<Unit>();
        protected readonly CompositeDisposable disposables = new CompositeDisposable();
        protected IObservable<Unit> destroy => _destroy;
        protected virtual void OnDispose()
        {
            this._destroy.OnNext(Unit.Default);
            this.disposables.Dispose();
        }
        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    OnDispose();
                }
                disposedValue = true;
            }
        }
        public void Dispose()
        => Dispose(true);
        #endregion
    }
}
