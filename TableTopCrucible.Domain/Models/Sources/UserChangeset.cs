using System;
using System.Collections.Generic;
using TableTopCrucible.Domain.ValueTypes.IDs;

namespace TableTopCrucible.Domain.Models.Sources
{
    public interface IUserChangeset : IEntityChangeset<User, UserId>
    {

    }
    public class UserChangeset : EntityChangesetBase<User, UserId>, IUserChangeset
    {

        public UserChangeset(User? origin = null) : base(origin)
        {
        }
        public override User Apply() => throw new NotImplementedException();
        public override IEnumerable<string> GetErrors() => throw new NotImplementedException();
        public override User ToEntity() => throw new NotImplementedException();
    }
}
