using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Subjects;

using TableTopCrucible.Core.Enums;
using TableTopCrucible.Core.Utilities;
using TableTopCrucible.Domain.Models.ValueTypes.IDs;

namespace TableTopCrucible.Core.Models.Sources
{
    public class AsyncJobState : DisposableReactiveObjectBase, IAsyncJobState
    {
        public AsyncJobId Id { get; } = AsyncJobId.New();
        public BehaviorSubject<IEnumerable<IAsyncProcessState>> ProcessChanges { get; } = new BehaviorSubject<IEnumerable<IAsyncProcessState>>(null);
        IObservable<IEnumerable<IAsyncProcessState>> IAsyncJobState.ProcessChanges => ProcessChanges;

        public BehaviorSubject<AsyncState> StateChanges { get; } = new BehaviorSubject<AsyncState>(AsyncState.ToDo);
        IObservable<AsyncState> IAsyncJobState.StateChanges => StateChanges;
        public AsyncState State => StateChanges.Value;

        public IEnumerable<IAsyncProcessState> Process { get; }

        public AsyncJobState()
        {
            this.disposables.Add(ProcessChanges, StateChanges);
        }
    }
}