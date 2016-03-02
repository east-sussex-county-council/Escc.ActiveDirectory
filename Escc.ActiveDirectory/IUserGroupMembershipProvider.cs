using System.Collections.Generic;
using System.Security.Principal;

namespace Escc.ActiveDirectory
{
    /// <summary>
    /// Look up whether a user is a member of an Active Directory group or groups
    /// </summary>
    public interface IUserGroupMembershipProvider
    {
        /// <summary>
        /// Tests whether the user is in one of the specified security groups
        /// </summary>
        /// <param name="groupMembershipProvider">The group membership provider.</param>
        /// <param name="groupNames">The group names, separated by semi-colons.</param>
        /// <exception cref="System.ArgumentNullException">groupMembershipProvider</exception>
        /// <returns></returns>
        bool UserIsInGroup(IUserGroupsProvider groupMembershipProvider, string groupNames);

        /// <summary>
        /// Tests whether the user is in one of the specified security groups
        /// </summary>
        /// <param name="groupMembershipProvider">The group membership provider.</param>
        /// <param name="groupNames">The group names.</param>
        /// <exception cref="System.ArgumentNullException">groupMembershipProvider</exception>
        /// <returns></returns>
        bool UserIsInGroup(IUserGroupsProvider groupMembershipProvider, IList<string> groupNames);

        /// <summary>
        /// Tests whether the user is in one of the specified security groups
        /// </summary>
        /// <param name="userIdentity">System.Security.Principal.WindowsIdentity</param>
        /// <param name="groupNames">The group names, separated by semi-colons.</param>
        /// <returns>a boolean</returns>
        bool UserIsInGroup(WindowsIdentity userIdentity, string groupNames);

        /// <summary>
        /// Tests whether the user is in one of the specified security groups
        /// </summary>
        /// <param name="userIdentity">System.Security.Principal.WindowsIdentity</param>
        /// <param name="groupNames">The group names.</param>
        /// <returns>a boolean</returns>
        bool UserIsInGroup(WindowsIdentity userIdentity, IList<string> groupNames);

        /// <summary>
        /// Tests whether the user is in the specified security groups, and returns a separate result for each group
        /// </summary>
        /// <param name="userIdentity">System.Security.Principal.WindowsIdentity</param>
        /// <param name="groupNames">The group names.</param>
        /// <returns>
        /// Returns group names and a boolean indicating membership.
        /// </returns>
        Dictionary<string, bool> UserIsInGroups(WindowsIdentity userIdentity, IEnumerable<string> groupNames);
    }
}