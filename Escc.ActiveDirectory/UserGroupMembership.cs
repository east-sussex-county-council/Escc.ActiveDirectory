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
    public class UserGroupMembership
    {
        private readonly IPermissionsResultCache _resultCache;
        private readonly IUserGroupsProvider _groupMembershipProvider;
        private readonly string _defaultDomain;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserGroupMembership" /> class.
        /// </summary>
        /// <param name="groupMembershipProvider">The group membership provider.</param>
        /// <param name="defaultDomain">The default domain.</param>
        /// <param name="resultCache">The result cache.</param>
        /// <exception cref="System.ArgumentNullException">groupMembershipProvider</exception>
        public UserGroupMembership(IUserGroupsProvider groupMembershipProvider, string defaultDomain=null, IPermissionsResultCache resultCache = null)
        {
            if (groupMembershipProvider == null) throw new ArgumentNullException("groupMembershipProvider");

            _groupMembershipProvider = groupMembershipProvider;
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
        /// <param name="groupNames">The group names, separated by semi-colons.</param>
        /// <returns></returns>
        public bool UserIsInGroup(string groupNames)
        {
            if (String.IsNullOrEmpty(groupNames)) throw new ArgumentNullException("groupNames", "groupNames cannot be null or an empty string");
            return UserIsInGroup(groupNames.Split(';'));
        }

        /// <summary>
        /// Tests whether the user is in one of the specified security groups
        /// </summary>
        /// <param name="groupNames">The group names.</param>
        /// <returns></returns>
        public bool UserIsInGroup(string[] groupNames)
        {
            if (groupNames == null || groupNames.Length == 0) throw new ArgumentNullException("groupNames", "groupNames cannot be null or an empty array");

            // If we've already done the check, return the previous result
            if (_resultCache != null)
            {
                var storedResult = _resultCache.CheckGroupMatchResult(groupNames);
                if (storedResult.HasValue) return storedResult.Value;
            }

            var len = groupNames.Length;
            for (var i = 0; i < len; i++)
            {
                groupNames[i] = groupNames[i].ToUpper(CultureInfo.CurrentCulture);
                if (!String.IsNullOrEmpty(_defaultDomain) && groupNames[i].StartsWith(_defaultDomain, StringComparison.Ordinal))
                {
                    groupNames[i] = groupNames[i].Substring(_defaultDomain.Length);
                }
            }

            // Add to List<string> to get access to .Contains method
            var queryGroups = new List<string>(groupNames);
            var groupsForUser = _groupMembershipProvider.GetGroupNames();

            foreach (string group in groupsForUser)
            {
                var groupName = group;
                if (!String.IsNullOrEmpty(_defaultDomain) && groupName.StartsWith(_defaultDomain, StringComparison.Ordinal))
                {
                    groupName = groupName.Substring(_defaultDomain.Length);
                }
                if (queryGroups.Contains(groupName))
                {
                    // Stash the result so we don't have to do this check again
                    if (_resultCache != null) _resultCache.SaveGroupMatchResult(groupNames, true);
                    return true;
                }
            }

            if (_resultCache != null) _resultCache.SaveGroupMatchResult(groupNames, false);
            return false;
        }
    }
}
