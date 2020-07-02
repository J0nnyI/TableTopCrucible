using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace TableTopCrucible.WPF.Helper
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
    }
}
