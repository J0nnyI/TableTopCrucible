using System;
using System.Reactive.Subjects;

using TableTopCrucible.Core.Enums;
using TableTopCrucible.Core.Utilities;
using TableTopCrucible.Core.ValueTypes;

namespace TableTopCrucible.Core.Models.Sources
{
    public class AsyncProcessState : DisposableReactiveObjectBase, IAsyncProcessState
    {
        public BehaviorSubject<AsyncState> StateChanges { get; } = new BehaviorSubject<AsyncState>(AsyncState.ToDo);
        IObservable<AsyncState> IAsyncProcessState.StateChanges => StateChanges;

        public BehaviorSubject<string> TextChanges { get; } = new BehaviorSubject<string>("untitled task");
        IObservable<string> IAsyncProcessState.TextChanges => TextChanges;

        public BehaviorSubject<string> DetailsChanges { get; } = new BehaviorSubject<string>("untitled task");
        IObservable<string> IAsyncProcessState.DetailsChanges => DetailsChanges;

        public BehaviorSubject<Progress?> ProgressChanges { get; } = new BehaviorSubject<Progress?>(null);
        IObservable<Progress?> IAsyncProcessState.ProgressChanges => ProgressChanges;

        public AsyncProcessState()
        {
            this.disposables.Add(StateChanges, TextChanges, DetailsChanges, ProgressChanges);
        }
    }
}