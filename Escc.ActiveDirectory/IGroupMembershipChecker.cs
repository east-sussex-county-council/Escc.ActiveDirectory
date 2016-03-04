using System.Collections.Generic;
using System.Security.Principal;

namespace Escc.ActiveDirectory
{
    /// <summary>
    /// Look up whether a user is a member of an Active Directory group or groups
    /// </summary>
    public interface IGroupMembershipChecker
    {
        /// <summary>
        /// Tests whether the user is in the specified security group
        /// </summary>
        /// <param name="groupName">The group name</param>
        /// <returns></returns>
        bool UserIsInGroup(string groupName);
        
        /// <summary>
        /// Tests whether the user is in one of the specified security groups
        /// </summary>
        /// <param name="groupNames">The group names.</param>
        /// <returns></returns>
        bool UserIsInGroup(IList<string> groupNames);

        /// <summary>
        /// Tests whether the user is in the specified security groups, and returns a separate result for each group
        /// </summary>
        /// <param name="groupNames">The group names.</param>
        /// <returns>
        /// Returns group names and a boolean indicating membership.
        /// </returns>
        Dictionary<string, bool> UserIsInGroups(IEnumerable<string> groupNames);
    }
}