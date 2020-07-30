using DynamicData;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using TableTopCrucible.Core.Models.Enums;
using TableTopCrucible.Core.Models.Sources;
using TableTopCrucible.Core.Models.ValueTypes.IDs;
using TableTopCrucible.Core.Services;
using TableTopCrucible.WPF.Helper;

namespace TableTopCrucible.Data.Services
{
    public class DataServiceBase<Tentity, Tid, Tchangeset> : IDataService<Tentity, Tid, Tchangeset>
        where Tentity : struct, IEntity<Tid>
        where Tid : ITypedId
        where Tchangeset : IEntityChangeset<Tentity, Tid>
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
            => keys.ChunkBy(settingsService.MaxPatchSize)
                .ToList()
                .ForEach(x => this.cache.Remove(x));
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
        public void Post(IEnumerable<Tentity> entities)
        {
            var job = this.notificationCenter.CreateSingleTaskJob(out var process, $"patching {entities.Count()} entities of type {typeof(Tentity).Name}");
            var chunks = entities.ChunkBy(settingsService.MaxPatchSize)
                  .ToList();
            process.AddProgress(chunks.Count);
            process.State = AsyncState.InProgress;
            try
            {
                process.State = AsyncState.InProgress;
                chunks.ForEach(x =>
                {
                    process.OnNextStep("posting ...");
                    this.cache.AddOrUpdate(x);
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

        protected DataServiceBase(
            ISettingsService settingsService,
            INotificationCenterService notificationCenter)
        {
            _readOnlyCache = cache.AsObservableCache();
            this.cache.DisposeWith(disposables);
            this.settingsService = settingsService;
            this.notificationCenter = notificationCenter;
        }
        void IDataService<Tentity, Tid, Tchangeset>.Clear() => this.cache.Clear();

        #region IDisposable Support
        private bool _disposedValue = false; // To detect redundant calls
        void IDataService<Tentity, Tid, Tchangeset>.Set(IEnumerable<Tentity> data)
        {
            this.cache.Clear();
            this.Post(data);
        }

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
}
