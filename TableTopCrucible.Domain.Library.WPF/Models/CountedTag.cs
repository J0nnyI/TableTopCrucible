using ReactiveUI;

using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;

using TableTopCrucible.Core.Models.Sources;
using TableTopCrucible.Data.Models.ValueTypes;

namespace TableTopCrucible.Domain.Library.WPF.Models
{
    public class CountedTag : DisposableReactiveObjectBase
    {
        public CountedTag(IObservable<int> totalChanges, IObservable<int> countChanges, Tag tag)
        {
            Tag = tag;
            this._count = null;
            this._total = null;


            this._count = countChanges
                .TakeUntil(destroy)
                .ToProperty(this, m => m.Count);

            this._total = totalChanges
                .TakeUntil(destroy)
                .ToProperty(this, m => m.Total);

            _portionPercent = Observable.CombineLatest(countChanges, totalChanges)
                .TakeUntil(destroy)
                .Select((values) => (double)values[0] / (double)values[1])
                .ToProperty(this, nameof(PortionPercent));
        }

        private readonly ObservableAsPropertyHelper<double> _portionPercent;
        public double PortionPercent => _portionPercent.Value;

        /**
         * the length of the source list
         */
        public int Total => _total.Value;
        private readonly ObservableAsPropertyHelper<int> _total;
        /**
         * the occurances of this tag in the source list
         */
        public int Count => _count.Value;
        private readonly ObservableAsPropertyHelper<int> _count;
        public Tag Tag { get; }

    }
}
