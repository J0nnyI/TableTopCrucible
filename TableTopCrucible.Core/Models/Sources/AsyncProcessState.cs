using System;
using System.Reactive.Subjects;

using TableTopCrucible.Core.Enums;
using TableTopCrucible.Core.ValueTypes;

namespace TableTopCrucible.Core.Models.Sources
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