using System.Collections.Generic;

namespace Escc.ActiveDirectory
{
    /// <summary>
    /// Gets a user's group memberships
    /// </summary>
    public interface IUserGroupsProvider
    {
        /// <summary>
        /// Gets the names of the groups a user is a member of.
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GetGroupNames();
    }
}