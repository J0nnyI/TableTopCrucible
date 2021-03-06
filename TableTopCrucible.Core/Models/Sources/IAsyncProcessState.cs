﻿using System;

using TableTopCrucible.Core.Models.Enums;
using TableTopCrucible.Core.ValueTypes;

namespace TableTopCrucible.Core.Models.Sources
{
    public interface IAsyncProcessState
    {
        IObservable<AsyncState> StateChanges { get; }
        AsyncState State { get; }
        IObservable<string> TitleChanges { get; }
        string Title { get; }
        Progress? Progress { get; }
        IObservable<string> DetailsChanges { get; }
        string Details { get; }
        IObservable<string> ErrorChanges { get; }
        IObservable<AsyncState> OnComplete { get; }

        string Errors { get; }
        void Complete();
    }
}