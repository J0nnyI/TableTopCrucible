using System.Diagnostics.CodeAnalysis;

namespace TableTopCrucible.Domain.Models.Views
{
    public struct View<Ta, Tb>
    {
        public View([AllowNull]Ta firstCompound, [AllowNull]Tb secondCompound)
        {
            this.FirstCompound = firstCompound;
            this.SecondCompound = secondCompound;
        }

        public Ta FirstCompound { get; }
        public Tb SecondCompound { get; }

    }
}
