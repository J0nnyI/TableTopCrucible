﻿using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Text;
using TableTopCrucible.Domain.Models;
using TableTopCrucible.Domain.ValueTypes.IDs;
using ReactiveUI;
using ReactiveUI.Wpf;
using DynamicData;

namespace TableTopCrucible.Domain.Services
{
    public interface IDataService<Tentity, Tid, TChangeset> :IDisposable
        where Tentity : IEntity<Tid> 
        where Tid : ITypedId
    {
        public IObservable<IChangeSet<Tentity,Tid>> Get();
        public IObservable<Change<Tentity,Tid>> Get(Tid id);
        public IObservable<IChangeSet<Tentity,Tid>> Get(IEnumerable<Tid> ids);
        public Tentity Patch(TChangeset data);
        public IEnumerable<Tentity> Patch(IEnumerable<TChangeset> data);
        public void Delete(Tid key);
        public void Delete(IEnumerable<Tid> key);
    }
}
