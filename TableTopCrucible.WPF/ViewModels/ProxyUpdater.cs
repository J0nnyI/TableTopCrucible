using DynamicData;
using DynamicData.Binding;

using System;

using TableTopCrucible.Domain.Models;
using TableTopCrucible.Domain.ValueTypes.IDs;
namespace TableTopCrucible.WPF.ViewModels
{
    public class ProxyUpdater<Tentity, Tid> : IObservableCollectionAdaptor<Tentity, Tid>
        where Tentity : IEntity<Tid>
        where Tid : ITypedId
    {
        private Action<IChangeSet<Tentity, Tid>, IObservableCollection<Tentity>> _action;
        public ProxyUpdater(Action<IChangeSet<Tentity, Tid>, IObservableCollection<Tentity>> action)
        {
            this._action = action;
        }
        public void Adapt(IChangeSet<Tentity, Tid> changes, IObservableCollection<Tentity> collection)
            => this._action(changes, collection);
    }
}
