using System;
using System.ComponentModel;

namespace TableTopCrucible.Core.Models.Sources
{
    public interface ISaveFileProgression : INotifyPropertyChanged
    {
        ITaskProgressionInfo DirectoryTaskState { get; }
        ITaskProgressionInfo ImageFileTaskState { get; }
        ITaskProgressionInfo ModelFileTaskState { get; }
        ITaskProgressionInfo LinkTaskState { get; }
        ITaskProgressionInfo ItemTaskState { get; }
        ITaskProgressionInfo MainTaskProgression { get; }
        TaskProgressionState SubTaskProgression { get; }
        IObservable<TaskProgressionState> SubTaskProgressionChanges { get; }

    }
}