﻿using DynamicData;

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
        Tentity Patch(TChangeset data);
        IEnumerable<Tentity> Patch(IEnumerable<TChangeset> data);
        bool CanPatch(TChangeset changeset);
        void Delete(Tid key);
        void Delete(IEnumerable<Tid> key);
        bool CanDelete(Tid key);
    }
}