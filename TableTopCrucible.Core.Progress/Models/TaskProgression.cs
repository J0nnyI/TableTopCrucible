using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;

using TableTopCrucible.Core.Helper;
using TableTopCrucible.Core.Models.Sources;
using TableTopCrucible.Core.Progress.Enums;

namespace TableTopCrucible.Core.Progress.Models
{

    public class TaskProgression : ReactiveObject, ITaskProgressionInfo
    {

        [Reactive]
        public TaskState State { get; set; } = TaskState.Todo;
        [Reactive]
        public string Details { get; set; } = string.Empty;
        [Reactive]
        public string Title { get; set; } = string.Empty;
        [Reactive]
        public int CurrentProgress { get; set; } = 0;
        [Reactive]
        public int RequiredProgress { get; set; } = 1;
        [Reactive]
        public Exception Error { get; set; }

        public IObservable<TaskState> TaskStateChanges { get; }
        public IObservable<string> DetailsChanges { get; }
        public IObservable<string> TitleChanges { get; }
        public IObservable<int> CurrentProgressChanges { get; }
        public IObservable<int> RequiredProgressChanges { get; }
        public IObservable<TaskProgressionState> TaskProgressionStateChanges { get; }
        public IObservable<TaskState> DoneChanges { get; }
        public IObservable<Exception> ErrorChanges { get; }

        public TaskProgression()
        {
            this.TaskStateChanges = this.WhenAnyValue(m => m.State);
            this.DetailsChanges = this.WhenAnyValue(m => m.Details);
            this.TitleChanges = this.WhenAnyValue(m => m.Title);
            this.CurrentProgressChanges = this.WhenAnyValue(m => m.CurrentProgress);
            this.RequiredProgressChanges = this.WhenAnyValue(m => m.RequiredProgress);
            this.ErrorChanges = this.WhenAnyValue(m => m.Error);
            this.TaskProgressionStateChanges = this.WhenAnyValue(
                m => m.State,
                m => m.Details,
                m => m.CurrentProgress,
                m => m.RequiredProgress)
                .Select((items)=>new TaskProgressionState(items.Item1,items.Item2,items.Item3,items.Item4));
            this.DoneChanges = TaskStateChanges.Where(state => state.IsIn(TaskState.Done, TaskState.Failed));
        }
    }
}
