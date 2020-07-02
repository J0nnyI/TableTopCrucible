using System;
using System.Collections.Generic;

using TableTopCrucible.Core.Models.Enums;
using TableTopCrucible.Domain.Models.ValueTypes.IDs;

namespace TableTopCrucible.Core.Models.Sources
{
    public interface IAsyncJobState
    {
        AsyncJobId Id { get; }
        string Title{ get; }
        IObservable<IEnumerable<IAsyncProcessState>> ProcessChanges { get; }
        IEnumerable<IAsyncProcessState> Processes { get; }
        IObservable<AsyncState> StateChanges { get; }
        AsyncState State { get; }
        void Complete();
    }
}