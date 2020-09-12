using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;

namespace TableTopCrucible.Core.Helper
{
    public static class CompositeDisposableHelper
    {
        public static void Add(this CompositeDisposable compDisposable, params IDisposable[] disposables)
            => compDisposable.Add(disposables as IEnumerable<IDisposable>);
        public static void Add(this CompositeDisposable compDisposable, IEnumerable<IDisposable> disposables)
            => disposables.ToList().ForEach(x => compDisposable.Add(x));
    }
}
