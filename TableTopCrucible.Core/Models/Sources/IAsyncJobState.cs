using System;
using System.Collections.Generic;

using TableTopCrucible.Core.Enums;

namespace TableTopCrucible.Core.Models.Sources
{
    public interface IAsyncJobState
    {
        IObservable<IEnumerable<IAsyncProcessState>> ProcessChanges { get; }
        IEnumerable<IAsyncProcessState> Process { get; }
        IObservable<AsyncState> StateChanges { get; }
        AsyncState State { get; }

    }
}