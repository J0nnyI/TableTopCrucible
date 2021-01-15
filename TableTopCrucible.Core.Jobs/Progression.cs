
using System;
using System.Reactive.Subjects;

using TableTopCrucible.Core.Models.Sources;

namespace TableTopCrucible.Core.Jobs
{

    public interface IProgression : IDisposable
    {
        decimal CurrentProgress { get; }
        IObservable<decimal> CurrentProgressChanges { get; }
        decimal TargetValue { get; }
        IObservable<decimal> TargetValueChanges { get; }
        string Title { get; }
        IObservable<string> TitleChanges { get; }
        string Details { get; }
        IObservable<string> DetailsChanges { get; }
    }
    public interface IProgressionController : IProgression
    {
        new decimal CurrentProgress { get; set; }
        new decimal TargetValue { get; set; }
        new string Title { get; set; }
        new string Details { get; set; }
    }

    public class Progression : DisposableReactiveObjectBase, IProgressionController
    {
        private readonly BehaviorSubject<decimal> _currentProgressChanges = new BehaviorSubject<decimal>(0);
        private readonly BehaviorSubject<decimal> _targetChanges = new BehaviorSubject<decimal>(1);
        private readonly BehaviorSubject<string> _detailsChanges = new BehaviorSubject<string>("");
        private readonly BehaviorSubject<string> _titleChanges = new BehaviorSubject<string>("");

        public IObservable<decimal> CurrentProgressChanges => _currentProgressChanges;
        public IObservable<decimal> TargetValueChanges => _targetChanges;
        public IObservable<string> TitleChanges => _titleChanges;
        public IObservable<string> DetailsChanges => _detailsChanges;

        public decimal CurrentProgress { get; set; }
        public decimal TargetValue { get; set; }
        public string Title { get; set; }
        public string Details { get; set; }

        public Progression(int targetValue, string title, string details)
        {
            this._targetChanges.OnNext(targetValue);
            this._titleChanges.OnNext(title);
            this._detailsChanges.OnNext(details);
        }
    }
}
