using System;

using TableTopCrucible.Core.Enums;
using TableTopCrucible.Core.ValueTypes;

namespace TableTopCrucible.Core.Models.Sources
{
    public interface IAsyncProcessState
    {
        IObservable<AsyncState> StateChanges { get; }
        IObservable<string> TextChanges { get; }
        IObservable<Progress?> ProgressChanges { get; }
        IObservable<string> DetailsChanges { get; }
    }
}