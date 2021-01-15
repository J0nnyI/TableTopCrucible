using DynamicData;
using DynamicData.Kernel;

using ReactiveUI;

using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace TableTopCrucible.Core.Jobs
{
    public enum JobState
    {
        ToDo,
        InProgress,
        Done,
        Failed
    }


    class Job<T> : ReactiveObject, IJobInfo<T>, IJobHandler<T>
    {
        private readonly BehaviorSubject<Optional<T>> _result = new BehaviorSubject<Optional<T>>(Optional.None<T>());
        private readonly SourceList<IProgression> _progression = new SourceList<IProgression>();

        public IObservable<T> Result => _result.Where(x => x.HasValue).Select(x => x.Value);
        public IObservableList<IProgression> Progression => _progression;
        public Job(Action<IJobHandler<T>> task)
        {
            Observable.Start(() =>
            {
                try
                {
                    task(this);
                }
                catch (Exception ex)
                {
                    this.Fail(ex);
                }
            }, RxApp.TaskpoolScheduler);
        }



        public void Complete(T result)
            => _result.OnNext(result);

        public void Fail(Exception ex)
            => _result.OnError(ex);

        public IProgressionController TrackProgression(int targetValue, string title, string details)
        {
            var prog = new Progression(targetValue, title, details);
            prog.Destroy
                .Take(1)
                .Delay(new TimeSpan(0, 0, 10))
                .Subscribe(_ =>
                    _progression.Remove(prog)
                );
            return prog;
        }
    }
    public interface IJobInfo
    {
        public IObservableList<IProgression> Progression { get; }

    }
    public interface IJobInfo<T> : IJobInfo
    {
        public IObservable<T> Result { get; }

    }
    public interface IJobHandler<T> : IJobInfo<T>
    {
        public IProgressionController TrackProgression(int targetValue, string title, string details);
        public void Complete(T result);
        public void Fail(Exception ex);
    }
}
