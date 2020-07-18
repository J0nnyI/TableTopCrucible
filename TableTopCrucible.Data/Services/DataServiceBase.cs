using DynamicData;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Subjects;

using TableTopCrucible.Core.Models.Sources;
using TableTopCrucible.Core.Models.ValueTypes.IDs;

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
        protected IObservable<Unit> destroy => _destroy;

        // delete
        public void Delete(Tid key)
            => cache.Remove(key);
        public void Delete(IEnumerable<Tid> keys)
            => cache.Remove(keys);
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
        public void Post(IEnumerable<Tentity> entity)
            => this.cache.AddOrUpdate(entity);
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
            cache.AddOrUpdate(changes);
            return changes;
        }
        public bool CanPatch(Tchangeset changeset)
        {
            if (!changeset.Origin.HasValue)
                return true;
            return this.cache.Keys.Contains(changeset.Origin.Value.Id);
        }

        protected DataServiceBase()
        {
            _readOnlyCache = cache.AsObservableCache();
            this.cache.DisposeWith(disposables);
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
