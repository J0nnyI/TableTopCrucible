
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

using TableTopCrucible.Core.Models.ValueTypes;
using TableTopCrucible.Core.Models.ValueTypes.IDs;

namespace TableTopCrucible.Core.Models.Sources
{
    public abstract class ReactiveEntityBase<Tself, Tentity, Tid> : DisposableReactiveValidationObject<Tself>, IEntityChangeset<Tentity, Tid>
        where Tentity : struct, IEntity<Tid>
        where Tid : ITypedId
        where Tself : ReactiveEntityBase<Tself, Tentity, Tid>
    {
        public Tentity? Origin { get; private set; }

        public abstract Tentity Apply();
        public virtual IEnumerable<string> GetErrors()
        {
            return Validators
                .Where(x => !x.IsValid(this as Tself))
                .Select(x => x.Message);
        }
        public abstract IEnumerable<Validator<Tself>> Validators { get; }
        public abstract Tentity ToEntity();
        public ReactiveEntityBase(Tentity? origin)
        {
            this.Origin = origin;
        }
    }

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
        protected Tchangeset getValue<Tchangeset, Torigin>(Tchangeset field, Torigin originalValue, Func<Torigin, Tchangeset> converter, [CallerMemberName] string propertyName = "")
            => changedValues.Contains(propertyName) || Origin == null ? field : converter(originalValue);

        protected T getValue<T>(T field, T originalValue, [CallerMemberName] string propertyName = "")
            => changedValues.Contains(propertyName) || Origin == null ? field : originalValue;
        protected T getStructValue<T>(T field, T? originalValue, [CallerMemberName] string propertyName = "") where T : struct
            => changedValues.Contains(propertyName) || Origin == null ? field : originalValue.Value;
        protected void setValue<Tchangeset, Torigin>(Tchangeset value, ref Tchangeset field, Torigin originalValue, Func<Tchangeset, Torigin, bool> comparer, [CallerMemberName] string propName = "")
        {
            if (value == null && originalValue == null || comparer(value, originalValue))
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
        protected void setValue<T>(T value, ref T field, T originalValue, [CallerMemberName] string propName = "")
            => this.setValue<T, T>(value, ref field, originalValue, (c, o) => c?.Equals(o) == true);

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
