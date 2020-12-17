using DynamicData;

using ReactiveUI;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Text;

using TableTopCrucible.Core.Models.Sources;
using TableTopCrucible.Core.Services;

namespace TableTopCrucible.Domain.Library.WPF.ViewModels
{
    public class NotificationCenterViewModel : DisposableReactiveObjectBase
    {
        private INotificationCenterService _notificationCenterService;

        private readonly ReadOnlyObservableCollection<IAsyncJobState> _jobs;
        public ReadOnlyObservableCollection<IAsyncJobState> Jobs => _jobs;

        public NotificationCenterViewModel(INotificationCenterService notificationCenterService)
        {
            this._notificationCenterService = notificationCenterService ?? throw new ArgumentNullException(nameof(notificationCenterService));
            this._notificationCenterService
                .GetJobs()
                .Connect()
                .TakeUntil(Destroy)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _jobs)
                .Subscribe();
        }
    }
}
