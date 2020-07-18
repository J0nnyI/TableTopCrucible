using DynamicData;

using System;
using System.Collections.Generic;

using TableTopCrucible.Core.Models.Sources;
using TableTopCrucible.Core.Models.ValueTypes.IDs;

namespace TableTopCrucible.Data.Services
{
    public interface IDataService<Tentity, Tid, TChangeset> : IDisposable
        where Tentity : IEntity<Tid>
        where Tid : ITypedId
    {
        IObservableCache<Tentity, Tid> Get();
        IObservable<Tentity> Get(Tid id);
        IObservable<IChangeSet<Tentity, Tid>> Get(IEnumerable<Tid> ids);

        public void Post(Tentity entity);
        public void Post(IEnumerable<Tentity> entity);
        Tentity Patch(TChangeset data);
        void Set(IEnumerable<Tentity> data);
        IEnumerable<Tentity> Patch(IEnumerable<TChangeset> data);
        bool CanPatch(TChangeset changeset);
        void Delete(Tid key);
        void Delete(IEnumerable<Tid> key);
        void Clear();
        bool CanDelete(Tid key);
    }
}
