using System;

using TableTopCrucible.Core.Enums;
using TableTopCrucible.Core.ValueTypes;

namespace TableTopCrucible.Core.Models.Sources
{
    public interface IAsyncProcessState
    {
        IObservable<AsyncState> StateChanges { get; }
        AsyncState State { get; }
        IObservable<string> TitleChanges { get; }
        string Title { get; }
        IObservable<Progress?> ProgressChanges { get; }
        Progress? Progress { get; }
        IObservable<string> DetailsChanges { get; }
        string Details { get; }
        IObservable<string> ErrorChanges { get; }
        string Errors { get; }
        void Complete();
    }
}