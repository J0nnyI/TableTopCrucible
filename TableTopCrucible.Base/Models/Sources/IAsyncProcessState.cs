using System;

using TableTopCrucible.Base.Enums;
using TableTopCrucible.Base.ValueTypes;

namespace TableTopCrucible.Base.Models.Sources
{
    public interface IAsyncProcessState
    {
        IObservable<AsyncState> StateChanges { get; }
        IObservable<string> TextChanges { get; }
        IObservable<Progress?> ProgressChanges { get; }
        IObservable<string> DetailsChanges { get; }
    }
}