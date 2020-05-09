using System;
using System.Collections.Generic;
using System.Text;

namespace TableTopCrucible.Domain.Models
{
    public interface IVersionedFile
    {
        Version Version { get; }

    }
}
