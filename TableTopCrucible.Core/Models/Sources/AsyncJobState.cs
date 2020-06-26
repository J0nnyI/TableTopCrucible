using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public string Title{ get; }
        public BehaviorSubject<IEnumerable<IAsyncProcessState>> ProcessChanges { get; } = new BehaviorSubject<IEnumerable<IAsyncProcessState>>(null);
        IObservable<IEnumerable<IAsyncProcessState>> IAsyncJobState.ProcessChanges => ProcessChanges;
        public ObservableAsPropertyHelper<IEnumerable<IAsyncProcessState>> _processes;
        public IEnumerable<IAsyncProcessState> Processes => _processes.Value;

        public BehaviorSubject<AsyncState> StateChanges { get; } = new BehaviorSubject<AsyncState>(AsyncState.ToDo);
        IObservable<AsyncState> IAsyncJobState.StateChanges => StateChanges;
        public AsyncState State => StateChanges.Value;


        public AsyncJobState(string title)
        {
            this.Title = title;
            this.disposables.Add(ProcessChanges, StateChanges);
            this._processes = this.ProcessChanges.ToProperty(this, nameof(Processes));
        }

        public void Complete()
        {
            this.ProcessChanges.Value.ToList().ForEach(x => x.Complete());
            this.ProcessChanges.OnCompleted();
            this.StateChanges.OnCompleted();

        }
    }
}