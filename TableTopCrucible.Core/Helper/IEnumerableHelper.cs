using DynamicData;

using ReactiveUI;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Text;

using TableTopCrucible.Core.Enums;
using TableTopCrucible.Core.Models.Sources;

namespace TableTopCrucible.Core.Helper
{
    public static class IEnumerableHelper
    {
        private class proxyComparer<T> : IEqualityComparer<T>, IEqualityComparer
        {
            private Func<T, T, bool> _comparer;
            private Func<T, int> _hashGenerator;
            public proxyComparer(Func<T, T, bool> comparer, Func<T, int> hashGenerator = null)
            {
                _comparer = comparer;
                this._hashGenerator = hashGenerator;
            }

            public bool Equals([AllowNull] T x, [AllowNull] T y) => _comparer(x, y);
            public new bool Equals(object x, object y) => x is T x2 && y is T y2 ? _comparer(x2, y2) : false;
            public int GetHashCode([DisallowNull] T obj) => _hashGenerator != null ? _hashGenerator(obj) : obj.GetHashCode();
            public int GetHashCode(object obj) => _hashGenerator != null && obj is T obje ? _hashGenerator(obje) : obj.GetHashCode();
        }

        public static IEnumerable<T> Distinct<T>(this IEnumerable<T> list, Func<T, T, bool> comparer)
            => list.Distinct(new proxyComparer<T>(comparer));
        /// <summary>
        /// ([1,2,3,4,5],2) => [[1,3,5],[2,4]]
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static IEnumerable<IGrouping<int, T>> SplitEvenly<T>(this IEnumerable<T> list, int count)
        {
            int i = 0;
            return list.GroupBy(_ => Convert.ToInt32(decimal.Remainder(i++, count)));
        }
        /// <summary>
        /// ([1,2,3,4,5],2) => [[1,2],[3,4],[5]]
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="maxSize"></param>
        /// <returns></returns>
        public static IEnumerable<IEnumerable<T>> ChunkBy<T>(this IEnumerable<T> list, int maxSize)
        {
            for (int i = 0; i < list.Count(); i += maxSize)
            {
                yield return list.Skip(i).Take(Math.Min(maxSize, list.Count() - i));
            }
        }
        public abstract class WhereInComparator<Tcmp>
        {
            public Tcmp Key { get; protected set; }
        }
        /**
         * { 1,2,3 }, {2,3,4} ==> {2,3}
         */
        public static IEnumerable<T> WhereIn<T, Tcmp>(this IEnumerable<T> list, IEnumerable<Tcmp> sublist, Func<T, Tcmp> selector)
        {
            return list.Where(x => sublist.Contains(selector(x)));
        }
        /**
         * { 1,2,3 }, {2,3,4} ==> {1}
         */
        public static IEnumerable<T> WhereNotIn<T, Tcmp>(this IEnumerable<T> list, IEnumerable<Tcmp> sublist, Func<T, Tcmp> selector)
        {
            return list.Where(x => !sublist.Contains(selector(x)));
        }
        public static bool ContainsAll<T>(this IEnumerable<T> list, IEnumerable<T> filter)
            => filter.All(x => list.Contains(x));
        /**
         * {1,2,3,4,5} + (2,4) ==> {2,3,4}
         * {1,2,3,4,5} + (4,2) ==> {2,3,4}
         */
        public static IEnumerable<T> Subsection<T>(this IEnumerable<T> list, T elementA, T elementB)
        {
            var indexA = list.IndexOf(elementA);
            var indexB = list.IndexOf(elementB);
            var first = indexA < indexB ? indexA : indexB;
            var last = indexA > indexB ? indexA : indexB;
            return list.Skip(first).Take(last - first + 1);
        }

        public static StringCollection ToStringCollection(this IEnumerable<string> list)
        {
            var fileList = new StringCollection();
            fileList.AddRange(list.ToArray());
            return fileList;
        }
        private static IEnumerable<Subject<T>> getSubjectList<T>(int count)
        {
            var res = new List<Subject<T>>();
            for (int i = 0; i < count; i++)
                res.Add(new Subject<T>());
            return res.ToArray();
        }
        public static IEnumerable<IObservable<IEnumerable<Tout>>> SelectAsync<Tin, Tout>(this IEnumerable<Tin> list, Func<Tin, Tout> selector, int chunkSize, out ITaskProgressionInfo progress)
        {
            var prog = new TaskProgression()
            {
                State = TaskState.Todo
            };
            var chunkCount = Convert.ToInt32(Math.Ceiling(Convert.ToDecimal(list.Count()) / chunkSize));
            var res = getSubjectList<IEnumerable<Tout>>(chunkCount);
            progress = prog;
            Observable.Start(() =>
            {
                try
                {
                    prog.State = TaskState.InProgress;
                    var chunks = list.ChunkBy(chunkSize).ToArray();
                    prog.RequiredProgress = chunks.Length;
                    var i = 0;
                    foreach (var chunk in chunks)
                    {
                        res.ElementAt(i).OnNext(chunk.Select(selector).ToArray());
                        prog.CurrentProgress++;
                        i++;
                    }
                    prog.State = TaskState.Done;
                }
                catch (Exception ex)
                {
                    prog.Details = "failed: " + ex.ToString();
                    prog.Error = ex;
                    prog.State = TaskState.Failed;
                }
            }, RxApp.TaskpoolScheduler);
            return res;
        }

        public static ITaskProgressionInfo ForEachAsync<T>(this IEnumerable<T> list, Action<T> action, int chunkSize)
        {
            SelectAsync(list, x =>
            {
                action(x);
                return new Unit();
            }, chunkSize, out var prog);
            return prog;
        }
    }
}
