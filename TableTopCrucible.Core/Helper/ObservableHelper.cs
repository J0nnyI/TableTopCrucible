using DynamicData.Kernel;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Text;

namespace TableTopCrucible.Core.Helper
{
    public struct PreviousAndCurrentValue<T>
    {
        public PreviousAndCurrentValue(Optional<T> previous, Optional<T> current)
        {
            Previous = previous;
            Current = current;
        }

        public Optional<T> Previous { get; }
        public Optional<T> Current { get; }
    }
    public static class ObservableHelper
    {
        public static IObservable<PreviousAndCurrentValue<T>> Pairwise<T>(this IObservable<T> src)
        {
            var prev = Optional<T>.None;
            return src
                .Select(value => new PreviousAndCurrentValue<T>(prev, value))
                .Do(cs => prev = cs.Current);
        }
    }
}
