using DynamicData;

using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Concurrency;

using TableTopCrucible.Core.Models.Sources;
using TableTopCrucible.Core.Models.ValueTypes.IDs;

namespace TableTopCrucible.Data.Services
{
    public interface IDataService<Tentity, Tid> : IDisposable
        where Tentity : IEntity<Tid>
        where Tid : ITypedId
    {
        IObservableCache<Tentity, Tid> Get();
        IObservable<Tentity> Get(Tid id);
        IObservable<IChangeSet<Tentity, Tid>> Get(IEnumerable<Tid> ids);

        public void Post(Tentity entity);
        ITaskProgressionInfo Post(IEnumerable<Tentity> entity, IScheduler scheduler = null, int? patchSize = null);
        ITaskProgressionInfo Set(IEnumerable<Tentity> data, IScheduler scheduler = null, int? patchSize = null);
        void Delete(Tid key);
        void Delete(IEnumerable<Tid> key);
        void Clear();
        bool CanDelete(Tid key);
        IObservable<Tentity> Get(IObservable<Tid> idChanges);
    }
    public interface IDataService<Tentity, Tid, TChangeset> :  IDataService<Tentity, Tid>
    where Tentity : IEntity<Tid>
    where Tid : ITypedId
    {
        Tentity Patch(TChangeset data);
        IObservable<IEnumerable<Tentity>> Patch(IEnumerable<TChangeset> changeSet, IScheduler scheduler = null);
        bool CanPatch(TChangeset changeset);
    }
}
