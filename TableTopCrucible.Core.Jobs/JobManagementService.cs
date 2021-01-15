using DynamicData;

using ReactiveUI;

using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;

namespace TableTopCrucible.Core.Jobs
{
    public interface IJobManagementService
    {
        IObservableList<IJobInfo> Jobs { get; }
        IJobInfo<T> Start<T>(Action<IJobHandler<T>> job);
    }
    public class JobManagementService: IJobManagementService
    {
        private readonly SourceList<IJobInfo> _jobs = new SourceList<IJobInfo>();
        public IObservableList<IJobInfo> Jobs => _jobs;
        public IJobInfo<T> Start<T>(Action<IJobHandler<T>> task)
        {
            var job = new Job<T>(task);
            this._jobs.Add(job);
            return job;
        }
    }
#if FALSE
    static class Demo
    {
        public static void testc()
        {
            IJobManagementService srv = new JobManagementService();
            srv.Start<string>(job =>
            {
                using (var mainProg = job.TrackProgression(10, "main counter", "just counting"))
                {

                    for (int y = 0; y < 10; y++)
                    {
                        mainProg.Details = y.ToString();

                        if (y > 10)
                            job.Fail(new Exception());
                        
                        using var subProg = job.TrackProgression(10, "subProg", "just counting some more");

                        for (int x = 0; x < 10; x++)
                        {
                            subProg.CurrentProgress++;
                            subProg.Details = y.ToString();
                        }


                    }
                    mainProg.CurrentProgress++;
                }

                job.Complete("success");
            });
        }
    }
#endif
}


