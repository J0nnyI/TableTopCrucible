using ReactiveUI;

using System;
using System.Reactive;
using System.Reactive.Subjects;

namespace TableTopCrucible.Domain.Models
{
    public class DisposableReactiveObject : ReactiveObject, IDisposable
    {
        private readonly Subject<Unit> _destroy = new Subject<Unit>();
        protected IObservable<Unit> destroy => _destroy;
        protected virtual void OnDispose()
        {
            this._destroy.OnNext(Unit.Default);
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
