
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

using TableTopCrucible.Core.Models.Sources;
using TableTopCrucible.Core.Models.ValueTypes;
using TableTopCrucible.Core.Models.ValueTypes.IDs;

namespace TableTopCrucible.Data.Models.Sources
{

    public abstract class EntityChangesetBase<Tentity, Tid> : IEntityChangeset<Tentity, Tid>, INotifyPropertyChanged
        where Tentity : struct, IEntity<Tid>
        where Tid : ITypedId
    {
        public Tentity? Origin { get; private set; }
        private List<string> _changedValues = new List<string>();
        protected IEnumerable<string> changedValues
            => _changedValues;

        public Tid Id { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public EntityChangesetBase(Tentity? origin)
        {
            Origin = origin;
        }
        protected Tchangeset getValue<Tchangeset, Torigin>(Tchangeset field, Torigin originalValue, Func<Torigin, Tchangeset> converter, [CallerMemberName] string propertyName = "")
            => changedValues.Contains(propertyName) || Origin == null ? field : converter(originalValue);

        protected T getValue<T>(T field, T originalValue, [CallerMemberName] string propertyName = "")
            => changedValues.Contains(propertyName) || Origin == null ? field : originalValue;
        protected T getStructValue<T>(T field, T? originalValue, [CallerMemberName] string propertyName = "") where T : struct
            => changedValues.Contains(propertyName) || Origin == null ? field : originalValue.Value;
        protected void setValue<Tchangeset, Torigin>(Tchangeset value, ref Tchangeset field, Torigin originalValue, Func<Tchangeset, Torigin, bool> comparer, [CallerMemberName] string propName = "")
        {
            if (value == null && originalValue == null || comparer(value, originalValue))
                _changedValues.Remove(propName);
            else
            {
                if (!_changedValues.Contains(propName))
                    _changedValues.Add(propName);
            }
            if (value?.Equals(field) != true)
            {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
            }
        }
        protected void setValue<T>(T value, ref T field, T originalValue, [CallerMemberName] string propName = "")
            => setValue(value, ref field, originalValue, (c, o) => c?.Equals(o) == true);

        protected void setStructValue<T>(T value, ref T field, T? originalValue, [CallerMemberName] string propName = "")
            where T : struct
        {
            if (Origin != null)
            {
                if (value.Equals(originalValue.Value) == true)
                    _changedValues.Remove(propName);
                else
                {
                    if (!_changedValues.Contains(propName))
                        _changedValues.Add(propName);
                }
            }
            if (value.Equals(field) != true)
            {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
            }
        }

        public abstract Tentity Apply();
        public abstract Tentity ToEntity();
        public abstract IEnumerable<string> GetErrors();
    }
}
