using System;
using System.Reactive;

namespace TableTopCrucible.Core.Services
{
    public interface ISaveService
    {
        void Load(string file);
        IObservable<Unit> Save(string file);
    }
}
