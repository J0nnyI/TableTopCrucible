﻿using TableTopCrucible.Domain.Models.Sources;
using TableTopCrucible.Domain.Models.ValueTypes.IDs;

namespace TableTopCrucible.Domain.Services
{
    public interface IUserDataService : IDataService<User, UserId, IUserChangeset> { }
    public class UserDataService : DataServiceBase<User, UserId, IUserChangeset>, IUserDataService
    {
    }
}
