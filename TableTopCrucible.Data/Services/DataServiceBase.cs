using DynamicData;

using ReactiveUI;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.InteropServices.ComTypes;
using System.Windows;

using TableTopCrucible.Core.Enums;
using TableTopCrucible.Core.Helper;
using TableTopCrucible.Core.Models.Enums;
using TableTopCrucible.Core.Models.Sources;
using TableTopCrucible.Core.Models.ValueTypes.IDs;
using TableTopCrucible.Core.Services;
using TableTopCrucible.Core.ValueTypes;
using TableTopCrucible.WPF.Helper;

namespace TableTopCrucible.Data.Services
{
    public class DataServiceBase<Tentity, Tid> : IDataService<Tentity, Tid>
        where Tentity : struct, IEntity<Tid>
        where Tid : ITypedId
    {

        // fields
        protected readonly CompositeDisposable disposables = new CompositeDisposable();
        protected SourceCache<Tentity, Tid> cache
            = new SourceCache<Tentity, Tid>(entity => entity.Id);
        private IObservableCache<Tentity, Tid> _readOnlyCache;
        private readonly SubjectBase<Unit> _destroy = new Subject<Unit>();
        private readonly ISettingsService settingsService;
        private readonly INotificationCenterService notificationCenter;

        protected IObservable<Unit> destroy => _destroy;

        // delete
        public void Delete(Tid key)
            => cache.Remove(key);
        public void Delete(IEnumerable<Tid> keys)
        {
            var job = this.notificationCenter.CreateSingleTaskJob(out var process, $"deleting {keys.Count()} entities of type {typeof(Tentity).Name}");
            var chunks = keys.ChunkBy(settingsService.MaxPatchSize)
                  .ToList();
            process.AddProgress(chunks.Count);
            process.State = AsyncState.InProgress;
            try
            {
                process.State = AsyncState.InProgress;
                chunks.ForEach(x =>
                {
                    process.OnNextStep("removing ...");
                    this.cache.Remove(x);
                });
                process.State = AsyncState.Done;
                process.Details = "done";
            }
            catch (Exception ex)
            {
                process.State = AsyncState.Failed;
                process.Details = ex.ToString();
            }
            finally
            {
                job.Dispose();
            }
        }
        public void Delete(Func<Tentity, bool> selector)
                    => this.Delete(this.cache.Items.Where(selector).Select(x => x.Id));
        public void Delete(Func<Tentity, bool> selector, out IEnumerable<Tentity> deletedItems)
        {
            deletedItems = this.cache.Items.Where(selector).ToArray();
            this.Delete(deletedItems.Select(x => x.Id));
        }

        public virtual bool CanDelete(Tid key)
            => cache.Keys.Contains(key);
        // get
        public IObservableCache<Tentity, Tid> Get()
            => _readOnlyCache;
        public IObservable<Tentity> Get(Tid id)
            => this.cache.WatchValue(id);
        public IObservable<IChangeSet<Tentity, Tid>> Get(IEnumerable<Tid> ids)
            => _readOnlyCache.Connect().Filter(item => ids.Contains(item.Id));
        // post
        public void Post(Tentity entity)
            => this.cache.AddOrUpdate(entity);
        public ITaskProgressionInfo Post(IEnumerable<Tentity> entities, IScheduler scheduler = null)
        {
            var progInfo = new TaskProgression();
            var chunks = entities.ChunkBy(settingsService.MaxPatchSize)
                  .ToList();
            progInfo.RequiredProgress = chunks.Count;
            Observable.Start(() =>
            {
                progInfo.State = TaskState.InProgress;
                try
                {
                    progInfo.Details = "posting ...";
                    chunks.ForEach(x =>
                    {
                        this.cache.AddOrUpdate(x);
                        progInfo.CurrentProgress++;
                    });
                    progInfo.State = TaskState.Done;
                    progInfo.Details = "done";
                }
                catch (Exception ex)
                {
                    progInfo.State = TaskState.Failed;
                    progInfo.Details = ex.ToString();
                    progInfo.Error = ex;
                }
            }, scheduler ?? RxApp.MainThreadScheduler);

            return progInfo;
        }

        protected DataServiceBase(
            ISettingsService settingsService,
            INotificationCenterService notificationCenter)
        {
            _readOnlyCache = cache.AsObservableCache();
            this.cache.DisposeWith(disposables);
            this.settingsService = settingsService;
            this.notificationCenter = notificationCenter;
        }
        public void Clear() => this.cache.Clear();

        private bool _disposedValue = false; // To detect redundant calls
        public ITaskProgressionInfo Set(IEnumerable<Tentity> data, IScheduler scheduler = null)
        {
            this.cache.Clear();
            return this.Post(data, scheduler);
        }

        #region IDisposable Support
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                    // dispose
                    this.cache.Dispose();
                // large fields, unmanaged
                this.cache = null;
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }


        #endregion
    }
    public class DataServiceBase<Tentity, Tid, Tchangeset> : DataServiceBase<Tentity, Tid>, IDataService<Tentity, Tid, Tchangeset>
        where Tentity : struct, IEntity<Tid>
        where Tid : ITypedId
        where Tchangeset : IEntityChangeset<Tentity, Tid>
    {
        protected DataServiceBase(ISettingsService settingsService, INotificationCenterService notificationCenter) : base(settingsService, notificationCenter)
        {
        }

        // patch
        public Tentity Patch(Tchangeset change)
        {
            Tentity entity;
            if (change.Origin.HasValue)
                entity = change.Apply();
            else
                entity = change.ToEntity();
            cache.AddOrUpdate(entity);
            return entity;
        }
        public IEnumerable<Tentity> Patch(IEnumerable<Tchangeset> changeSet)
        {
            var changes = changeSet.Select(change =>
                change.Origin != null
                ? change.Apply()
                : change.ToEntity());
            this.Post(changes);
            return changes;
        }
        public bool CanPatch(Tchangeset changeset)
        {
            if (!changeset.Origin.HasValue)
                return true;
            return this.cache.Keys.Contains(changeset.Origin.Value.Id);
        }

    }
}
