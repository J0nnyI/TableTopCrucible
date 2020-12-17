﻿using ReactiveUI;

using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Subjects;

namespace TableTopCrucible.Core.Models.Sources
{
    public abstract class DisposableReactiveObjectBase : ReactiveObject, IDisposable
    {
        private readonly Subject<Unit> _destroy = new Subject<Unit>();
        protected readonly CompositeDisposable disposables = new CompositeDisposable();
        public IObservable<Unit> Destroy => _destroy;
        protected virtual void onDispose() { }

        #region IDisposable Support
        public bool IsDisposed { get; private set; } = false; // To detect redundant calls

        private void _dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    onDispose();
                    this._destroy.OnNext(Unit.Default);
                    this._destroy.Dispose();
                    this.disposables.Dispose();
                }
                IsDisposed = true;
            }
        }
        public void Dispose()
            => _dispose(true);
        #endregion
    }
}
