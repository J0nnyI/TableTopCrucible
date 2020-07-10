using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Text;

using TableTopCrucible.Core.Models.Sources;

namespace TableTopCrucible.Core.Services
{
    public interface ISettingsService
    {
        void Save();
        int ThreadCount { get; set; }
        int MaxPatchSize { get; set; }
    }
}
