using ReactiveUI;

using System;
using System.Reactive.Subjects;
using System.Windows.Threading;

using TableTopCrucible.Core.Models.Enums;
using TableTopCrucible.Core.Utilities;
using TableTopCrucible.Core.ValueTypes;

namespace TableTopCrucible.Core.Models.Sources
{
    public class AsyncProcessState : DisposableReactiveObjectBase, IAsyncProcessState
    {
        public BehaviorSubject<AsyncState> StateChanges { get; } = new BehaviorSubject<AsyncState>(AsyncState.ToDo);
        IObservable<AsyncState> IAsyncProcessState.StateChanges => StateChanges;
        private readonly ObservableAsPropertyHelper<AsyncState> _state;
        public AsyncState State
        {
            get => _state.Value;
            set => _dispatch(() => StateChanges.OnNext(value));
        }



        public BehaviorSubject<string> TitleChanges { get; } = new BehaviorSubject<string>(null);
        IObservable<string> IAsyncProcessState.TitleChanges => TitleChanges;
        public string Title
        {
            get => _title.Value;
            set => _dispatch(() => TitleChanges.OnNext(value));
        }
        private readonly ObservableAsPropertyHelper<string> _title;



        public BehaviorSubject<string> DetailsChanges { get; } = new BehaviorSubject<string>(null);
        IObservable<string> IAsyncProcessState.DetailsChanges => DetailsChanges;
        public string Details
        {
            get => _details.Value;
            set => _dispatch(() => DetailsChanges.OnNext(value));
        }
        private readonly ObservableAsPropertyHelper<string> _details;



        public BehaviorSubject<Progress?> ProgressChanges { get; } = new BehaviorSubject<Progress?>(null);
        IObservable<Progress?> IAsyncProcessState.ProgressChanges => ProgressChanges;
        public Progress? Progress
        {
            get => _progress.Value;
            set => _dispatch(() => ProgressChanges.OnNext(value));
        }
        private readonly ObservableAsPropertyHelper<Progress?> _progress;



        public BehaviorSubject<string> ErrorChanges { get; } = new BehaviorSubject<string>(null);
        IObservable<string> IAsyncProcessState.ErrorChanges => ErrorChanges;
        public string Errors
        {
            get => _errors.Value;
            set => _dispatch(() => ErrorChanges.OnNext(value));
        }
        private readonly ObservableAsPropertyHelper<string> _errors;


        private readonly Dispatcher _dispatcher = null;

        public AsyncProcessState(string title = "untitled process", string details = "", Dispatcher dispatcher = null)
        {
            this._dispatcher = dispatcher;
            this._state = StateChanges.ToProperty(this, nameof(State));
            this._title = TitleChanges.ToProperty(this, nameof(Title));
            this._progress = ProgressChanges.ToProperty(this, nameof(Progress));
            this._details = DetailsChanges.ToProperty(this, nameof(Details));
            this._errors = ErrorChanges.ToProperty(this, nameof(Errors));

            this.Title = title;
            this.Details = details;
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
        public void AddProgress(int stepCount, string message = null)
        {
            _dispatch(() =>
            {
                this.ProgressChanges.OnNext(new Progress(0, stepCount));
                if (message != null)
                    this.Details += message + Environment.NewLine;
            });
        }
        public void OnNextStep(string message = null)
        {
            _dispatch(() =>
            {
                ProgressChanges.OnNext(Progress.Value.OnNextStep());
                if (message != null)
                    this.Details = message;
            });
        }
        private void _dispatch(Action action)
        {
            if (this._dispatcher != null)
            {
                this._dispatcher.Invoke(action);
            }
            else
                action();
        }
    }
}