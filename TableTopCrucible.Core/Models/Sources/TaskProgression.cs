using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;

using TableTopCrucible.Core.Enums;
using TableTopCrucible.Core.Helper;

namespace TableTopCrucible.Core.Models.Sources
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
            TaskStateChanges = this.WhenAnyValue(m => m.State);
            DetailsChanges = this.WhenAnyValue(m => m.Details);
            TitleChanges = this.WhenAnyValue(m => m.Title);
            CurrentProgressChanges = this.WhenAnyValue(m => m.CurrentProgress);
            RequiredProgressChanges = this.WhenAnyValue(m => m.RequiredProgress);
            ErrorChanges = this.WhenAnyValue(m => m.Error);
            TaskProgressionStateChanges = 
                this.WhenAnyValue(
                    m => m.Title,
                    m => m.State,
                    m => m.Details,
                    m => m.CurrentProgress,
                    m => m.RequiredProgress,
                    m => m.Error,
                    (t,s,d,c,r,e)=>new TaskProgressionState(t,s,d,c,r,e));
            DoneChanges = TaskStateChanges.Where(state => state.IsIn(TaskState.Done, TaskState.Failed));
        }
    }
}
