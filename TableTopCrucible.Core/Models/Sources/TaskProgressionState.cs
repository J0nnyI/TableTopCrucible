
using System;

using TableTopCrucible.Core.Enums;

namespace TableTopCrucible.Core.Models.Sources
{
    public struct TaskProgressionState
    {
        public TaskProgressionState(string title, TaskState state, string details, int currentProgress, int requiredProgress)
        {
            State = state;
            Details = details;
            CurrentProgress = currentProgress;
            RequiredProgress = requiredProgress;
            Title = title;
        }

        public TaskState State { get; }
        public string Details { get; }
        public string Title { get; }
        public int CurrentProgress { get; }
        public int RequiredProgress { get; }
        public override bool Equals(object obj)
        {
            if(obj is TaskProgressionState state2)
            {
                return this.State == state2.State &&
                    this.Details == state2.Details &&
                    this.Title == state2.Title &&
                    this.CurrentProgress == state2.CurrentProgress &&
                    this.RequiredProgress == state2.RequiredProgress;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(State, Details, Title, CurrentProgress, RequiredProgress);
        }
    }
}
