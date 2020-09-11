using DynamicData;
using DynamicData.Binding;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reactive.Linq;
using System.Text;

namespace TableTopCrucible.Core.Utilities
{
    public static class DynamicDataHelper
    {
        private class Sorter<T> : IComparer<T>
        {
            private readonly Func<T, T, int> comparer;

            public Sorter(Func<T, T, int> comparer)
            {
                this.comparer = comparer;
            }
            public int Compare([AllowNull] T x, [AllowNull] T y) => comparer(x, y);
        }

        public static IObservable<ISortedChangeSet<TObject, TKey>> Sort<TObject, TKey>(this IObservable<IChangeSet<TObject, TKey>> source, IObservable<Func<TObject, TObject, int>> comparer)
            => source.Sort(comparer.Select(comp => new Sorter<TObject>(comp)));

        public static IObservable<ISortedChangeSet<TObject, TKey>> Sort<TObject, TKey>(this IObservable<IChangeSet<TObject, TKey>> source, Func<TObject, TObject, int> comparer)
            => source.Sort(Observable.Return(new Sorter<TObject>(comparer)));

        public static IObservable<ISortedChangeSet<TObject, TKey>> Sort<TObject, TKey>(this IObservable<IChangeSet<TObject, TKey>> source,
                                                                                       SortDirection sortDirection = SortDirection.Ascending) where TObject : IComparable
            => source.Sort((a, b) => a.CompareTo(b) * (sortDirection == SortDirection.Descending ? -1 : 1));
        public static IObservable<ISortedChangeSet<TObject, TKey>> Sort<TObject, TKey>(this IObservable<IChangeSet<TObject, TKey>> source, Func<TObject, IComparable> textRetriever, SortDirection sortDirection = SortDirection.Ascending)
        => source.Sort((a, b) => textRetriever(a).CompareTo(textRetriever(b)) *
            (sortDirection == SortDirection.Descending ? -1 : 1));
    }
}
