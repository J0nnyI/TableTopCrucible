using ReactiveUI.Validation.Helpers;

using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Subjects;

namespace TableTopCrucible.Domain.Models
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
        public bool IsDisposed { get; private set; } = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    OnDispose();
                }
                IsDisposed = true;
            }
        }
        public void Dispose()
        => Dispose(true);
        #endregion
    }
}
