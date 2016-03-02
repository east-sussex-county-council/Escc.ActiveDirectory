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
    public class UserGroupMembershipProvider : IUserGroupMembershipProvider
    {
        private readonly IPermissionsResultCache _resultCache;
        private readonly string _defaultDomain;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserGroupMembershipProvider" /> class.
        /// </summary>
        /// <param name="defaultDomain">The default domain.</param>
        /// <param name="resultCache">The result cache.</param>
        public UserGroupMembershipProvider(string defaultDomain=null, IPermissionsResultCache resultCache = null)
        {
            _resultCache = resultCache;

            // default domain prefix to remove if present, normalised to uppercase
            if (!String.IsNullOrEmpty(defaultDomain))
            {
                _defaultDomain = defaultDomain.ToUpperInvariant() + "\\";
            }
        }

        /// <summary>
        /// Tests whether the user is in one of the specified security groups
        /// </summary>
        /// <param name="groupMembershipProvider">The group membership provider.</param>
        /// <param name="groupNames">The group names, separated by semi-colons.</param>
        /// <exception cref="System.ArgumentNullException">groupMembershipProvider</exception>
        /// <returns></returns>
        public bool UserIsInGroup(IUserGroupsProvider groupMembershipProvider, string groupNames)
        {
            if (groupMembershipProvider == null) throw new ArgumentNullException(nameof(groupMembershipProvider));
            if (String.IsNullOrEmpty(groupNames)) throw new ArgumentNullException(nameof(groupNames), "groupNames cannot be null or an empty string");
            return UserIsInGroup(groupMembershipProvider, new List<string>(groupNames.Split(';')));
        }

        /// <summary>
        /// Tests whether the user is in one of the specified security groups
        /// </summary>
        /// <param name="groupMembershipProvider">The group membership provider.</param>
        /// <param name="groupNames">The group names.</param>
        /// <exception cref="System.ArgumentNullException">groupMembershipProvider</exception>
        /// <returns></returns>
        public bool UserIsInGroup(IUserGroupsProvider groupMembershipProvider, IList<string> groupNames)
        {
            if (groupMembershipProvider == null) throw new ArgumentNullException(nameof(groupMembershipProvider));
            if (groupNames == null || groupNames.Count == 0) throw new ArgumentNullException(nameof(groupNames), "groupNames cannot be null or an empty list");

            // If we've already done the check, return the previous result
            if (_resultCache != null)
            {
                var storedResult = _resultCache.CheckGroupMatchResult(groupNames);
                if (storedResult.HasValue) return storedResult.Value;
            }

            var len = groupNames.Count;
            for (var i = 0; i < len; i++)
            {
                groupNames[i] = groupNames[i].ToUpper(CultureInfo.CurrentCulture);
                if (!String.IsNullOrEmpty(_defaultDomain) && groupNames[i].StartsWith(_defaultDomain, StringComparison.Ordinal))
                {
                    groupNames[i] = groupNames[i].Substring(_defaultDomain.Length);
                }
            }

            // Add to List<string> to get access to .Contains method
            var groupsForUser = groupMembershipProvider.GetGroupNames();

            foreach (string group in groupsForUser)
            {
                var groupName = group;
                if (!String.IsNullOrEmpty(_defaultDomain) && groupName.StartsWith(_defaultDomain, StringComparison.Ordinal))
                {
                    groupName = groupName.Substring(_defaultDomain.Length);
                }
                if (groupNames.Contains(groupName))
                {
                    // Stash the result so we don't have to do this check again
                    if (_resultCache != null) _resultCache.SaveGroupMatchResult(groupNames, true);
                    return true;
                }
            }

            if (_resultCache != null) _resultCache.SaveGroupMatchResult(groupNames, false);
            return false;
        }

        /// <summary>
        /// Tests whether the user is in one of the specified security groups
        /// </summary>
        /// <param name="userIdentity">System.Security.Principal.WindowsIdentity</param>
        /// <param name="groupNames">The group names, separated by semi-colons.</param>
        /// <returns>a boolean</returns>
        public bool UserIsInGroup(WindowsIdentity userIdentity, string groupNames)
        {
            WindowsPrincipal wp = new WindowsPrincipal(userIdentity);
            return UserIsInGroup(userIdentity, groupNames.Split(';'));
        }

        /// <summary>
        /// Tests whether the user is in one of the specified security groups
        /// </summary>
        /// <param name="userIdentity">System.Security.Principal.WindowsIdentity</param>
        /// <param name="groupNames">The group names.</param>
        /// <returns>a boolean</returns>
        public bool UserIsInGroup(WindowsIdentity userIdentity, IList<string> groupNames)
        {
            if (userIdentity == null || groupNames == null || groupNames.Count == 0)
            {
                return false;
            }

            WindowsPrincipal wp = new WindowsPrincipal(userIdentity);
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
        /// <param name="userIdentity">System.Security.Principal.WindowsIdentity</param>
        /// <param name="groupNames">The group names.</param>
        /// <returns>
        /// Returns group names and a boolean indicating membership.
        /// </returns>
        public Dictionary<string, bool> UserIsInGroups(WindowsIdentity userIdentity, IEnumerable<string> groupNames)
        {
            var groupMembershipCollection = new Dictionary<string, bool>();
            WindowsPrincipal wp = new WindowsPrincipal(userIdentity);
            if (groupNames != null)
            {
                foreach (string group in groupNames)
                {
                    var isInRole = wp.IsInRole(group);
                    groupMembershipCollection.Add(group, isInRole);
                }
            }

            return groupMembershipCollection;
        }
    }
}
