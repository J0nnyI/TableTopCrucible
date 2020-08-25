using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using System;
using System.Reactive.Linq;
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
        IObservable<AsyncState> IAsyncProcessState.StateChanges => StateChanges.ObserveOn(RxApp.MainThreadScheduler);
        private readonly ObservableAsPropertyHelper<AsyncState> _state;
        public AsyncState State
        {
            get => _state.Value;
            set => StateChanges.OnNext(value);
        }



        public BehaviorSubject<string> TitleChanges { get; } = new BehaviorSubject<string>(null);
        IObservable<string> IAsyncProcessState.TitleChanges => TitleChanges.ObserveOn(RxApp.MainThreadScheduler);
        public string Title
        {
            get => _title.Value;
            set => TitleChanges.OnNext(value);
        }
        private readonly ObservableAsPropertyHelper<string> _title;



        private BehaviorSubject<string> _detailsChanges { get; } = new BehaviorSubject<string>(null);
        public IObservable<string> DetailsChanges => _detailsChanges.ObserveOn(RxApp.MainThreadScheduler);
        public string Details
        {
            get => _details.Value;
            set => _detailsChanges.OnNext(value);
        }
        private readonly ObservableAsPropertyHelper<string> _details;



        public BehaviorSubject<Progress?> _progressChanges { get; } = new BehaviorSubject<Progress?>(null);
        public IObservable<Progress?> ProgressChanges => _progressChanges.ObserveOn(RxApp.MainThreadScheduler);
        public Progress? Progress
        {
            get => _progress.Value;
            set => _progressChanges.OnNext(value);
        }
        private readonly ObservableAsPropertyHelper<Progress?> _progress;



        public BehaviorSubject<string> ErrorChanges { get; } = new BehaviorSubject<string>(null);
        IObservable<string> IAsyncProcessState.ErrorChanges => ErrorChanges.ObserveOn(RxApp.MainThreadScheduler);
        public string Errors
        {
            get => _errors.Value;
            set => ErrorChanges.OnNext(value);
        }
        private readonly ObservableAsPropertyHelper<string> _errors;



        public AsyncProcessState(string title = "untitled process", string details = "")
        {
            this._state = StateChanges.ToProperty(this, nameof(State));
            this._title = TitleChanges.ToProperty(this, nameof(Title));
            this._details = _detailsChanges.ToProperty(this, nameof(Details));
            this._errors = ErrorChanges.ToProperty(this, nameof(Errors));
            this._progress = ProgressChanges.ToProperty(this, nameof(Progress));

            this.Title = title;
            this.Details = details;
            this.disposables.Add(StateChanges, TitleChanges, _detailsChanges, ErrorChanges, _progressChanges);
        }

        public void Complete()
        {
            this.StateChanges.OnCompleted();
            this.TitleChanges.OnCompleted();
            this._detailsChanges.OnCompleted();
            this.ErrorChanges.OnCompleted();
        }
        public void AddProgress(int stepCount, string message = null)
        {
            this.Progress = new Progress(0, stepCount);
            if (message != null)
                this.Details += message + Environment.NewLine;
        }
        public void OnNextStep(string message = null)
        {
            Observable.Start(() =>
            {
                if (!_progressChanges.Value.HasValue)
                    throw new InvalidOperationException($"progress has not been initialized jet (job {this.Title}, {this.Details})");
                Progress = _progressChanges.Value.Value.OnNextStep();
                if (message != null)
                    this.Details = message;
            }, RxApp.MainThreadScheduler);
        }
        public void Skip(int count, string message = null)
        {
            Observable.Start(() =>
            {
                if (!_progressChanges.Value.HasValue)
                    throw new InvalidOperationException($"progress has not been initialized jet (job {this.Title}, {this.Details})");
                Progress = _progressChanges.Value.Value.Skip(count);
                if (message != null)
                    this.Details = message;
            }, RxApp.MainThreadScheduler);
        }
    }
}