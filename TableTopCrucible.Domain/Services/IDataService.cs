using System;
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
    public interface IDataService<T, Tid, TChangeset> :IDisposable
        where T : IEntity<Tid> 
        where Tid : ITypedId
    {
        public IObservable<IChangeSet<T,Tid>> Get();
        public IObservable<Change<T,Tid>> Get(Tid id);
        public IObservable<IChangeSet<T,Tid>> Get(IEnumerable<Tid> ids);
        public void Patch(TChangeset data);
        public void Patch(IEnumerable<TChangeset> data);
        public void Delete(Tid key);
        public void Delete(IEnumerable<Tid> key);
    }
}
