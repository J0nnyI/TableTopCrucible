using DynamicData;
using DynamicData.Binding;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;

namespace TableTopCrucible.Core.Helper
{
    public static class DynamicDataHelper
    {
        #region sort
        private class Sorter<T> : IComparer<T>
        {
            private readonly Func<T, T, int> comparer;

            public Sorter(Func<T, T, int> comparer)
            {
                this.comparer = comparer;
            }
            public int Compare([AllowNull] T x, [AllowNull] T y) => comparer(x, y);
        }

        public static IObservable<ISortedChangeSet<TObject, TKey>> Sort<TObject, TKey>(
            this IObservable<IChangeSet<TObject, TKey>> source,
            IObservable<Func<TObject, TObject, int>> comparer)
            => source.Sort(comparer.Select(comp => new Sorter<TObject>(comp)));
        public static IObservable<IChangeSet<TObject>> Sort<TObject, TKey>(
            this IObservable<IChangeSet<TObject>> source,
            IObservable<Func<TObject, TObject, int>> comparer)
            => source.Sort(comparer.Select(comp => new Sorter<TObject>(comp)));




        public static IObservable<ISortedChangeSet<TObject, TKey>> Sort<TObject, TKey>(this IObservable<IChangeSet<TObject, TKey>> source, Func<TObject, TObject, int> comparer)
            => source.Sort(Observable.Return(new Sorter<TObject>(comparer)));
        public static IObservable<IChangeSet<TObject>> Sort<TObject>(this IObservable<IChangeSet<TObject>> source, Func<TObject, TObject, int> comparer)
            => source.Sort(Observable.Return(new Sorter<TObject>(comparer)));




        public static IObservable<ISortedChangeSet<TObject, TKey>> Sort<TObject, TKey>(this IObservable<IChangeSet<TObject, TKey>> source,
                                                                                       SortDirection sortDirection = SortDirection.Ascending) where TObject : IComparable
            => source.Sort((a, b) => a.CompareTo(b) * (sortDirection == SortDirection.Descending ? -1 : 1));
        public static IObservable<IChangeSet<TObject>> Sort<TObject>(this IObservable<IChangeSet<TObject>> source,
                                                                                       SortDirection sortDirection = SortDirection.Ascending) where TObject : IComparable
            => source.Sort((a, b) => a.CompareTo(b) * (sortDirection == SortDirection.Descending ? -1 : 1));



        public static IObservable<ISortedChangeSet<TObject, TKey>> Sort<TObject, TKey>(this IObservable<IChangeSet<TObject, TKey>> source, Func<TObject, IComparable> textRetriever, SortDirection sortDirection = SortDirection.Ascending)
        => source.Sort((a, b) => textRetriever(a).CompareTo(textRetriever(b)) *
            (sortDirection == SortDirection.Descending ? -1 : 1));
        public static IObservable<IChangeSet<TObject>> Sort<TObject>(this IObservable<IChangeSet<TObject>> source, Func<TObject, IComparable> textRetriever, SortDirection sortDirection = SortDirection.Ascending)
        => source.Sort((a, b) => textRetriever(a).CompareTo(textRetriever(b)) *
            (sortDirection == SortDirection.Descending ? -1 : 1));
        #endregion


        #region filter

        public static IObservable<IChangeSet<TObject, TKey>> Filter<TObject, TKey>(this IObservable<IChangeSet<TObject, TKey>> source, Func<TObject, bool> filter, IObservable<Unit> reapplyFilter) =>
            source.Filter(reapplyFilter.Select(_ => filter));

        #endregion

        #region adapt
        private class remoteChangeSetAdaptor<TObject, TKey> : IChangeSetAdaptor<TObject, TKey>
        {
            private readonly Action<IChangeSet<TObject, TKey>> action;

            public remoteChangeSetAdaptor(Action<IChangeSet<TObject, TKey>> action)
                => this.action = action;
            public void Adapt(IChangeSet<TObject, TKey> change)
                => action(change);
        }

        public static IObservable<IChangeSet<TObject, TKey>> Adapt<TObject, TKey>(this IObservable<IChangeSet<TObject, TKey>> source, Action<IChangeSet<TObject, TKey>> adaptor)
            => source.Adapt(new remoteChangeSetAdaptor<TObject, TKey>(adaptor));

        #endregion

        public static void HanldeManyChanges<TObject, TKey>(
            this IChangeSet<TObject, TKey> changes,
            Action<IEnumerable<Change<TObject, TKey>>> handleAdds,
            Action<IEnumerable<Change<TObject, TKey>>> handleRemoves = null,
            Action<IEnumerable<Change<TObject, TKey>>> handleUpdates = null,
            Action<IEnumerable<Change<TObject, TKey>>> handleMoves = null,
            Action<IEnumerable<Change<TObject, TKey>>> handleRefreshs = null,
            Action handleNoChanges = null)
        {
            if (changes.Any() == false)
            {
                handleNoChanges?.Invoke();
                return;
            }

            var groups = changes.GroupBy(change => change.Reason);

            var adds = groups.FirstOrDefault(x => x.Key == ChangeReason.Add);
            if (adds?.Any() == true)
                handleAdds?.Invoke(adds);
            var removes = groups.FirstOrDefault(x => x.Key == ChangeReason.Remove);

            if (removes?.Any() == true)
                handleRemoves?.Invoke(removes);
            var updates = groups.FirstOrDefault(x => x.Key == ChangeReason.Update);

            if (updates?.Any() == true)
                handleUpdates?.Invoke(updates);
            var moves = groups.FirstOrDefault(x => x.Key == ChangeReason.Moved);

            if (moves?.Any() == true)
                handleMoves?.Invoke(moves);
            var refreshs = groups.FirstOrDefault(x => x.Key == ChangeReason.Refresh);

            if (refreshs?.Any() == true)
                handleRefreshs?.Invoke(refreshs);

        }

        public static void HandleChanges<TObject, TKey>(
            this IChangeSet<TObject, TKey> changes,
            Action<Change<TObject, TKey>> handleAdds,
            Action<Change<TObject, TKey>> handleRemove = null,
            Action<Change<TObject, TKey>> handleUpdate = null,
            Action<Change<TObject, TKey>> handleMove = null,
            Action<Change<TObject, TKey>> handleRefresh = null)
        {
            changes.ToList().ForEach(change =>
            {
                switch (change.Reason)
                {
                    case ChangeReason.Add:
                        handleAdds?.Invoke(change);
                        break;
                    case ChangeReason.Update:
                        handleRemove?.Invoke(change);
                        break;
                    case ChangeReason.Remove:
                        handleUpdate?.Invoke(change);
                        break;
                    case ChangeReason.Refresh:
                        handleMove?.Invoke(change);
                        break;
                    case ChangeReason.Moved:
                        handleRefresh?.Invoke(change);
                        break;
                    default:
                        throw new InvalidOperationException("HandleChanges");
                }
            });
        }

    }
}
