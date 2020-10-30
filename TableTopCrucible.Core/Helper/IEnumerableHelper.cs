using DynamicData;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;

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

        public static IEnumerable<IGrouping<int, T>> SplitEvenly<T>(this IEnumerable<T> list, int count)
        {
            int i = 0;
            return list.GroupBy(_ => Convert.ToInt32(decimal.Remainder(i++, count)));
        }
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


    }
}
