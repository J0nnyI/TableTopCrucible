using DynamicData.Annotations;

using MaterialDesignThemes.Wpf.Converters.CircularProgressBar;

using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;

using TableTopCrucible.Core.Models.Enums;
using TableTopCrucible.Core.Utilities;
using TableTopCrucible.WPF.Helper;

namespace TableTopCrucible.Core.ValueTypes
{
    public struct Progress
    {
        public Progress(int min, int value, int max)
        {
            this.Min = min;
            this.Value = value;
            this.Max = max;
        }
        public Progress(int progress, int taskCount)
        {
            this.Min = 0;
            this.Value = progress;
            this.Max = taskCount;
        }

        public int Min { get; }
        public int Value { get; }
        public int Max { get; }

        public Progress OnNextStep()
            => new Progress(Min, Value + 1, Max);
    }




    //public class tester
    //{

    //    static void testc()
    //    {
    //        var job = new AsyncJob("test", () => { Console.Log("first task"); });
    //    }
    //}





    //public class AsyncJob
    //{
    //    List<AsyncThread> _starters = new List<AsyncThread>();
    //    public IEnumerable<AsyncThread> Starters => _starters;
    //    public AsyncJob(string name, Action<IAsyncFeedbackProvider> worker, bool autostart = false)
    //    {
    //        this.Name = name;
    //    }

    //    public string Name { get; }

    //    public void AddStarter(string name, Action<IAsyncFeedbackProvider> worker)
    //    {

    //    }
    //    public void Start()
    //    {

    //    }


    //}
    //public class AsyncThread
    //{
    //    public AsyncState State { get; }
    //    public IObservable<Progress> Progress { get; }
    //    Dispatcher UiDispatcher { get; }
    //    public void Start()
    //    {

    //    }
    //    public void AddSuccessor(string name, Action<IAsyncFeedbackProvider> action)
    //    {

    //    }

    //    public AsyncThread AddSuccessor<T>(string threadName, IEnumerable<T> list, Action<IEnumerable<T>> action, int maxSubSize)
    //    {
    //        this.AddSuccessor(
    //            $"{threadName}#{group.thread}",
    //            callback =>
    //            {
    //                foreach (var group in list.SplitEvenly(list.Count() / maxSubSize))
    //                {
    //                    group.subGroups.ToList().ForEach(subGroup =>
    //                        this.UiDispatcher.Invoke(() => action(subGroup))
    //                    );
    //                }
    //            }
    //        );
    //    }
    //}

    //public interface IAsyncFeedbackProvider
    //{
    //    Progress Progress { get; set; }

    //    Action Invoke { get; }
    //    string CurrentTask { get; }
    //    string
    //}




}