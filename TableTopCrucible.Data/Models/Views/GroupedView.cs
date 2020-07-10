using DynamicData.Kernel;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace TableTopCrucible.Domain.Models.Views
{
    public struct GroupedView<Ta, Tb>
    {
        public GroupedView(Ta firstCompound, IEnumerable<Tb> secondCompound)
        {
            this.FirstCompound = firstCompound;
            this.SecondCompound = secondCompound;
        }

        public Ta FirstCompound { get; }
        public IEnumerable<Tb> SecondCompound { get; }
    }
}
