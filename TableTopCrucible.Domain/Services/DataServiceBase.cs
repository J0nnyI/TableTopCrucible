using DynamicData;
using System;
using System.Collections.Generic;
using System.Text;
using TableTopCrucible.Domain.Models.Sources;
using TableTopCrucible.Domain.ValueTypes.IDs;
using System.Linq;
using TableTopCrucible.Domain.Models;
using System.Windows.Input;

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
        private IObservableCache<Tentity, Tid> readOnlyCache;

        // delete
        public void Delete(Tid key)
            => cache.Remove(key);
        public void Delete(IEnumerable<Tid> keys)
            => cache.Remove(keys);
        // get
        public IObservable<IChangeSet<Tentity, Tid>> Get()
            => cache.Connect();
        public IObservable<Change<Tentity, Tid>> Get(Tid id)
            => this.cache.Watch(id);
        public IObservable<IChangeSet<Tentity, Tid>> Get(IEnumerable<Tid> ids)
        {
            return this.cache.Connect().Filter(item => ids.Contains(item.Id));
        }
        // patch
        public void Patch(Tchangeset change)
        {
            if (change.Origin.HasValue)
                cache.AddOrUpdate(change.Apply());
            else
                cache.AddOrUpdate(change.ToEntity());
        }
        public void Patch(IEnumerable<Tchangeset> changeSet)
        {
            var changes = changeSet.Select(change =>
                change.Origin != null
                ? change.Apply()
                : change.ToEntity());
            cache.AddOrUpdate(changes);
        }

        protected DataServiceBase()
        {
            readOnlyCache = cache.AsObservableCache();
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // dispose
                    this.cache.Dispose();
                }
                // large fields, unmanaged
                this.cache = null;
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
