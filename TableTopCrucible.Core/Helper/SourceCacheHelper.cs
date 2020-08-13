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

        //cache
        public static IObservable<Tres?> WatchValue<Tres, Tkey>(
            this ISourceCache<Tres, Tkey> cache,
            IObservable<Tkey?> idChanges,
            Func<Tres, Tkey> selector = null) where Tkey : struct where Tres : struct
            => cache.WatchValue(idChanges, _optionalSelector(selector, cache.KeySelector));



        public static IObservable<Tres?> WatchValue<Tres, Tkey>(
            this ISourceCache<Tres, Tkey> cache,
            IObservable<Tkey?> idChanges,
            Func<Tres, Tkey?> selector = null) where Tkey : struct where Tres : struct
            => cache.Connect().WatchValue(idChanges, selector);

        //observer
        public static IObservable<Tres?> WatchValue<Tres, Tkey>(
            this IObservable<IChangeSet<Tres, Tkey>> cache,
            IObservable<Tkey?> idChanges,
            Func<Tres, Tkey> selector) where Tkey : struct where Tres : struct
            => cache.WatchValue(idChanges, val => (Tkey?)selector(val));


        public static IObservable<Tres?> WatchValue<Tres, Tkey>(
            this IObservable<IChangeSet<Tres, Tkey>> cache,
            IObservable<Tkey?> idChanges,
            Func<Tres, Tkey?> selector) where Tkey : struct where Tres : struct
        {
            var filter = idChanges.Select(version =>
                new Func<Tres, bool>(item => selector(item).Equals(version))
            );

            return cache
                .Filter(filter)
                .Select(x => x.Select(x => x.Current).Cast<Tres?>().FirstOrDefault());
        }

        //public static IObservable<IChangeSet<Tres, Tkey>> WatchValues
        //    <Tres, Tkey, Tfilter>
        //    (this IObservable<IChangeSet<)
        //    where Tres : struct
        //    where Tkey : struct
        //    where Tfilter : struct
        //{

        //}
    }
}
