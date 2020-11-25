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
        public static IObservable<PreviousAndCurrentValue<T>> PreviousAndCurrent<T>(this IObservable<T> src)
            => src.Aggregate(new PreviousAndCurrentValue<T>(Optional.None<T>(), Optional.None<T>()),
                (acc, val) => new PreviousAndCurrentValue<T>(acc.Current, val));
    }
}
