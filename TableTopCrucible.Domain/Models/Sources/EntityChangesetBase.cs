using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using TableTopCrucible.Domain.ValueTypes.IDs;

namespace TableTopCrucible.Domain.Models.Sources
{
    public abstract class EntityChangesetBase<Tentity, Tid> : IEntityChangeset<Tentity, Tid>, INotifyPropertyChanged
        where Tentity : struct, IEntity<Tid>
        where Tid : ITypedId
    {
        public Tentity? Origin { get; private set; }
        private List<string> _changedValues = new List<string>();
        protected IEnumerable<string> changedValues
            => _changedValues;

        public Tid Id { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        public EntityChangesetBase(Tentity? origin)
        {
            this.Origin = origin;
        }
        protected T getValue<T>(T field, T originalValue, [CallerMemberName]string propertyName = "")
            => changedValues.Contains(propertyName) || Origin == null ? field : originalValue;
        protected T getStructValue<T>(T field, T? originalValue, [CallerMemberName] string propertyName = "") where T : struct
        {
            return changedValues.Contains(propertyName) || Origin == null ? field : originalValue.Value;
        }
        protected void setValue<T>(T value, ref T field, T originalValue, [CallerMemberName] string propName = "")
        {
            if (value == null && originalValue == null || value?.Equals(originalValue) == true)
                this._changedValues.Remove(propName);
            else
            {
                if (!this._changedValues.Contains(propName))
                    this._changedValues.Add(propName);
            }
            if (value?.Equals(field) != true)
            {
                field = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
            }
        }
        protected void setStructValue<T>(T value, ref T field, T? originalValue, [CallerMemberName] string propName = "")
            where T : struct
        {
            if (this.Origin != null)
            {
                if (value.Equals(originalValue.Value) == true)
                    this._changedValues.Remove(propName);
                else
                {
                    if (!this._changedValues.Contains(propName))
                        this._changedValues.Add(propName);
                }
            }
            if (value.Equals(field) != true)
            {
                field = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
            }
        }

        public abstract Tentity Apply();
        public abstract Tentity ToEntity();
        public abstract IEnumerable<string> GetErrors();
    }
}
