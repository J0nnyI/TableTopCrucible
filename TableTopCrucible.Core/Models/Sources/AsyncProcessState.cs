using ReactiveUI;

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
        private readonly ObservableAsPropertyHelper<AsyncState> _state;
        public AsyncState State => _state.Value;



        public BehaviorSubject<string> TitleChanges { get; } = new BehaviorSubject<string>(null);
        IObservable<string> IAsyncProcessState.TitleChanges => TitleChanges;
        public string Title => _title.Value;
        private readonly ObservableAsPropertyHelper<string> _title;



        public BehaviorSubject<string> DetailsChanges { get; } = new BehaviorSubject<string>(null);
        IObservable<string> IAsyncProcessState.DetailsChanges => DetailsChanges;
        public string Details => _details.Value;
        private readonly ObservableAsPropertyHelper<string> _details;



        public BehaviorSubject<Progress?> ProgressChanges { get; } = new BehaviorSubject<Progress?>(null);
        IObservable<Progress?> IAsyncProcessState.ProgressChanges => ProgressChanges;
        public Progress? Progress => _progress.Value;
        private readonly ObservableAsPropertyHelper<Progress?> _progress;



        public BehaviorSubject<string> ErrorChanges { get; } = new BehaviorSubject<string>(null);
        IObservable<string> IAsyncProcessState.ErrorChanges => ErrorChanges;
        public string Errors => _errors.Value;
        private readonly ObservableAsPropertyHelper<string> _errors;


        public AsyncProcessState(string title = "untitled process", string details = "")
        {
            this._state = StateChanges.ToProperty(this, nameof(State));
            this._title= TitleChanges.ToProperty(this, nameof(Title));
            this._progress = ProgressChanges.ToProperty(this, nameof(Progress));
            this._details = DetailsChanges.ToProperty(this, nameof(Details));
            this._errors = ErrorChanges.ToProperty(this, nameof(Errors));

            this.TitleChanges.OnNext(title);
            this.DetailsChanges.OnNext(details);
            this.disposables.Add(StateChanges, TitleChanges, DetailsChanges, ProgressChanges, ErrorChanges);
        }

        public void Complete()
        {
            this.StateChanges.OnCompleted();
            this.TitleChanges.OnCompleted();
            this.DetailsChanges.OnCompleted();
            this.ProgressChanges.OnCompleted();
            this.ErrorChanges.OnCompleted();
        }
    }
}