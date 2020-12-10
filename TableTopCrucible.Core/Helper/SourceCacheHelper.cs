using DynamicData;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;

namespace TableTopCrucible.Core.Helper
{
    public static class SourceCacheHelper
    {
        private static Func<Tres, Tkey?> _optionalSelector<Tres, Tkey>(Func<Tres, Tkey> newSel, Func<Tres, Tkey> original) where Tkey : struct
        {
            if (newSel != null)
                return val => newSel(val);
            else
                return val => original(val);
        }

        public static IObservable<Func<Tcache, bool>> ToFilter
            <Tcache, Tobservable>
            (
            this IObservable<Tobservable> observable,
            Func<Tcache,Tobservable, bool> filter
            )
        {
            return observable.Select(value =>
                new Func<Tcache, bool>(item => filter(item,value))
            );
        }
        public static IObservable<TObject> WatchValue<TObject, TKey>(this IObservable<IChangeSet<TObject, TKey>> source, IObservable<TKey> keyChanges)
            => keyChanges.Select(key => source.WatchValue(key)).Switch();

    }
}
