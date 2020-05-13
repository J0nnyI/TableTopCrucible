using System;

namespace TableTopCrucible.Domain.Models.Sources
{
    public struct Destributor
    {
        public Destributor(string name)
        {
            this.Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public string Name { get; }
    }
}
