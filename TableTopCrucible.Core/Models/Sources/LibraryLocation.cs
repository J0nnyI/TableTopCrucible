using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

using TableTopCrucible.Core.Models.ValueTypes;

namespace TableTopCrucible.Core.Models.Sources
{
    [Serializable]
    public struct LibraryLocation
    {
        public LibraryLocation(bool isPinned, string path, DateTime lastUse)
        {
            IsPinned = isPinned;
            Path = path;
            LastUse = lastUse;
        }

        [Reactive]public bool IsPinned { get; set; }
        [Reactive]public string Path { get; set; }
        [Reactive]public DateTime LastUse { get; set; }
    }
}
