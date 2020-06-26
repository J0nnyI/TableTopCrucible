using System;
using System.Reactive.Subjects;

using TableTopCrucible.Base.Enums;
using TableTopCrucible.Base.ValueTypes;

namespace TableTopCrucible.Base.Models.Sources
{
    public class AsyncProcessState : IAsyncProcessState
    {
        public BehaviorSubject<AsyncState> StateChanges { get; } = new BehaviorSubject<AsyncState>(AsyncState.ToDo);
        IObservable<AsyncState> IAsyncProcessState.StateChanges => StateChanges;

        public BehaviorSubject<string> TextChanges { get; } = new BehaviorSubject<string>("untitled task");
        IObservable<string> IAsyncProcessState.TextChanges => TextChanges;

        public BehaviorSubject<string> DetailsChanges { get; } = new BehaviorSubject<string>("untitled task");
        IObservable<string> IAsyncProcessState.DetailsChanges => DetailsChanges;

        public BehaviorSubject<Progress?> ProgressChanges { get; } = new BehaviorSubject<Progress?>(null);
        IObservable<Progress?> IAsyncProcessState.ProgressChanges => ProgressChanges;
    }
}