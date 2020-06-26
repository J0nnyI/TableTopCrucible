using System;
using System.Collections.Generic;

using TableTopCrucible.Base.Enums;

namespace TableTopCrucible.Base.Models.Sources
{
    public interface IAsyncJobState
    {
        IObservable<IEnumerable<IAsyncProcessState>> ProcessChanges { get; }
        IEnumerable<IAsyncProcessState> Process { get; }
        IObservable<AsyncState> StateChanges { get; }
        AsyncState State { get; }

    }
}