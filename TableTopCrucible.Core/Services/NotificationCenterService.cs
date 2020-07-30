using DynamicData;

using System.Windows.Threading;

using TableTopCrucible.Core.Models.Sources;
using TableTopCrucible.Core.Utilities;
using TableTopCrucible.Domain.Models.ValueTypes.IDs;

namespace TableTopCrucible.Core.Services
{
    public interface INotificationCenterService
    {
        AsyncJobState CreateSingleTaskJob(out AsyncProcessState processState, string jobName, string processName = "", Dispatcher dispatcher =null);
        public void Register(IAsyncJobState job);
        public IObservableCache<IAsyncJobState, AsyncJobId> GetJobs();
    }
    public class NotificationCenterService : INotificationCenterService
    {
        private readonly SourceCache<IAsyncJobState, AsyncJobId> _jobs = new SourceCache<IAsyncJobState, AsyncJobId>(x => x.Id);



        public IObservableCache<IAsyncJobState, AsyncJobId> GetJobs()
            => _jobs;

        public void Register(IAsyncJobState job)
            => this._jobs.AddOrUpdate(job);

        public AsyncJobState CreateSingleTaskJob(out AsyncProcessState processState, string jobName, string processName = "untitled process", Dispatcher dispatcher = null)
        {
            var job = new AsyncJobState(jobName);
            processState = new AsyncProcessState(processName,"",dispatcher);
            job.ProcessChanges.OnNext(processState.AsArray());

            this.Register(job);
            return job;
        }
    }
}
