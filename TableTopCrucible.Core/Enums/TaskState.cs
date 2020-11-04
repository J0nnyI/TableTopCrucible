using System;
using System.Collections.Generic;
using System.Text;

namespace TableTopCrucible.Core.Enums
{
    public enum TaskState
    {
        Todo,
        Done,
        Failed,
        InProgress,
        RunningWithErrors,
        PartialSuccess
    }
}
