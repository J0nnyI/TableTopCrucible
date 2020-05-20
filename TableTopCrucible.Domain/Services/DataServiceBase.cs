using DynamicData;

using System;
using System.Collections.Generic;
using System.Linq;

using TableTopCrucible.Domain.Models;
using TableTopCrucible.Domain.Models.Sources;
using TableTopCrucible.Domain.ValueTypes.IDs;

namespace TableTopCrucible.Domain.Services
{
    public class DataServiceBase<Tentity, Tid, Tchangeset> : IDataService<Tentity, Tid, Tchangeset>
        where Tentity : struct, IEntity<Tid>
        where Tid : ITypedId
        where Tchangeset : IEntityChangeset<Tentity, Tid>
    {
        // fields
        protected SourceCache<Tentity, Tid> cache
            = new SourceCache<Tentity, Tid>(entity => entity.Id);
        private IObservableCache<Tentity, Tid> _readOnlyCache;

        // delete
        public void Delete(Tid key)
            => cache.Remove(key);
        public void Delete(IEnumerable<Tid> keys)
            => cache.Remove(keys);
        // get
        public IObservableCache<Tentity, Tid> Get()
            => _readOnlyCache;
        public IObservable<Tentity> Get(Tid id)
            => this.cache.WatchValue(id);
        public IObservable<IChangeSet<Tentity, Tid>> Get(IEnumerable<Tid> ids)
            => _readOnlyCache.Connect().Filter(item => ids.Contains(item.Id));
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

        protected DataServiceBase()
        {
            _readOnlyCache = cache.AsObservableCache();
        }

        #region IDisposable Support
        private bool _disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // dispose
                    this.cache.Dispose();
                }
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
