using DynamicData;

using TableTopCrucible.Core.Models.Sources;
using TableTopCrucible.Domain.Models.ValueTypes.IDs;

namespace TableTopCrucible.Core.Services
{
    public interface INotificationCenterService
    {

    }
    public class NotificationCenterService : INotificationCenterService
    {
        private SourceCache<AsyncJobState, AsyncJobId> _jobs = new SourceCache<AsyncJobState, AsyncJobId>(x => x.Id);


    }
}
