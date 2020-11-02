
using System;

using TableTopCrucible.Core.Enums;

namespace TableTopCrucible.Core.Models.Sources
{
    public interface ITaskProgressionInfo
    {
        TaskState State { get; }
        string Title { get; set; }
        string Details { get; }
        int CurrentProgress { get; }
        int RequiredProgress { get; }
        Exception Error { get; }
        IObservable<TaskState> TaskStateChanges { get; }
        IObservable<string> DetailsChanges { get; }
        IObservable<int> CurrentProgressChanges { get; }
        IObservable<int> RequiredProgressChanges { get; }
        IObservable<TaskProgressionState> TaskProgressionStateChanges { get; }
        IObservable<TaskState> DoneChanges { get; }
        IObservable<Exception> ErrorChanges { get; }
    }
}
