using DynamicData;

using System;
using System.Collections.Generic;

using TableTopCrucible.Domain.Models;
using TableTopCrucible.Domain.ValueTypes.IDs;

namespace TableTopCrucible.Domain.Services
{
    public interface IDataService<Tentity, Tid, TChangeset> : IDisposable
        where Tentity : IEntity<Tid>
        where Tid : ITypedId
    {
        public IObservableCache<Tentity, Tid> Get();
        public IObservable<Tentity> Get(Tid id);
        public IObservable<IChangeSet<Tentity, Tid>> Get(IEnumerable<Tid> ids);
        public Tentity Patch(TChangeset data);
        public IEnumerable<Tentity> Patch(IEnumerable<TChangeset> data);
        public void Delete(Tid key);
        public void Delete(IEnumerable<Tid> key);
    }
}
