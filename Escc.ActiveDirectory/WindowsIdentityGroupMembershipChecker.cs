using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Globalization;
using System.Security.Principal;
using System.Web;

namespace Escc.ActiveDirectory
{
    /// <summary>
    /// Checks the permissions of an Active Directory user 
    /// </summary>
    public class WindowsIdentityGroupMembershipChecker  : IGroupMembershipChecker
    {
        private readonly WindowsIdentity _userIdentity;

        /// <summary>
        /// Initializes a new instance of the <see cref="WindowsIdentityGroupMembershipChecker"/> class.
        /// </summary>
        /// <param name="userIdentity">The user identity.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public WindowsIdentityGroupMembershipChecker(WindowsIdentity userIdentity)
        {
            _userIdentity = userIdentity;
        }

        /// <summary>
        /// Tests whether the user is in the specified security group
        /// </summary>
        /// <param name="groupName">The group name</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public bool UserIsInGroup(string groupName)
        {
            var groupList = new List<string> {groupName};
            return UserIsInGroup(groupList);
        }

        /// <summary>
        /// Tests whether the user is in one of the specified security groups
        /// </summary>
        /// <param name="groupNames">The group names.</param>
        /// <returns>a boolean</returns>
        public bool UserIsInGroup(IList<string> groupNames)
        {
            if (_userIdentity == null || groupNames == null || groupNames.Count == 0)
            {
                return false;
            }

            WindowsPrincipal wp = new WindowsPrincipal(_userIdentity);
            foreach (var group in groupNames)
            {
                if (wp.IsInRole(group))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Tests whether the user is in the specified security groups, and returns a separate result for each group
        /// </summary>
        /// <param name="groupNames">The group names.</param>
        /// <returns>
        /// Returns group names and a boolean indicating membership.
        /// </returns>
        public Dictionary<string, bool> UserIsInGroups(IEnumerable<string> groupNames)
        {
            var groupMembershipCollection = new Dictionary<string, bool>();
            if (groupNames != null)
            {
                if (_userIdentity == null)
                {
                    foreach (var groupName in groupNames)
                    {
                        groupMembershipCollection.Add(groupName, false);
                    }
                    return groupMembershipCollection;
                }
                else
                {
                    WindowsPrincipal wp = new WindowsPrincipal(_userIdentity);
                    foreach (string group in groupNames)
                    {
                        var isInRole = wp.IsInRole(group);
                        groupMembershipCollection.Add(group, isInRole);
                    }
                }
            }

            return groupMembershipCollection;
        }
    }
}
