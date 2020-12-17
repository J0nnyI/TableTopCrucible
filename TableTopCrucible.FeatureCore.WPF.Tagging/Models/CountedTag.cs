using ReactiveUI;

using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;

using TableTopCrucible.Core.Models.Sources;
using TableTopCrucible.Data.Models.ValueTypes;

namespace TableTopCrucible.FeatureCore.WPF.Tagging.Models
{
    public class CountedTag : DisposableReactiveObjectBase, IComparable
    {
        public CountedTag(IObservable<int> totalChanges, IObservable<int> countChanges, Tag tag)
        {
            Tag = tag;
            _count = null;
            _total = null;


            _count = countChanges
                .TakeUntil(Destroy)
                .ToProperty(this, m => m.Count);

            _total = totalChanges
                .TakeUntil(Destroy)
                .ToProperty(this, m => m.Total);

            _portionPercent = Observable.CombineLatest(countChanges, totalChanges)
                .TakeUntil(Destroy)
                .Select((values) => values[0] / (double)values[1])
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

        public int CompareTo(object obj)
        {
            if (obj is CountedTag tagB)
            {
                var countCmp = Count.CompareTo(tagB.Count);
                if (countCmp != 0)
                    return countCmp;
                else
                    return Tag.CompareTo(tagB.Tag);
            }
            return -1;
        }
    }
}
