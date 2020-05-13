using System;

namespace TableTopCrucible.Domain.Models
{
    public interface IVersionedFile
    {
        Version Version { get; }

    }
}
