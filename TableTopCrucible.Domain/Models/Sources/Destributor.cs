using System;
using System.Collections.Generic;
using System.Text;

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
