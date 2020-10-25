using System;
using System.Reactive;

using TableTopCrucible.Core.Models.Sources;

namespace TableTopCrucible.Core.Services
{
    public interface ISaveService
    {
        ISaveFileProgression Load(string file);
        IObservable<Unit> Save(string file);
    }
}
