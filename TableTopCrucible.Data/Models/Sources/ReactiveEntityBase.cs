using System.Collections.Generic;
using System.Linq;

using TableTopCrucible.Core.Models.Sources;
using TableTopCrucible.Core.Models.ValueTypes;
using TableTopCrucible.Core.Models.ValueTypes.IDs;

namespace TableTopCrucible.Data.Models.Sources
{
    public abstract class ReactiveEntityBase<Tself, Tentity, Tid> : DisposableReactiveValidationObject, IEntityChangeset<Tentity, Tid>
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
            Origin = origin;
        }
    }
}
