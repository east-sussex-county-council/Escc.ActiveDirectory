using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Web;

namespace Escc.ActiveDirectory
{
    /// <summary>
    /// Gets a user's groups using the LogonUserIdentity in the current request using <see cref="HttpContext" />
    /// </summary>
    public class LogonIdentityGroupMembershipChecker : IGroupMembershipChecker
    {
        private readonly IPermissionsResultCache _resultCache;
        private readonly string _defaultDomain;
        private readonly WindowsIdentity _userIdentity;

        /// <summary>
        /// Initializes a new instance of the <see cref="LogonIdentityGroupMembershipChecker" /> class.
        /// </summary>
        /// <param name="defaultDomain">The default domain.</param>
        /// <param name="resultCache">The result cache.</param>
        public LogonIdentityGroupMembershipChecker(string defaultDomain = null, IPermissionsResultCache resultCache = null)
        {
            if (HttpContext.Current == null) throw new InvalidOperationException("LogonIdentityGroupMembershipChecker requires a valid HTTP context");

            _resultCache = resultCache;
            _userIdentity = HttpContext.Current.Request.LogonUserIdentity;

            // default domain prefix to remove if present, normalised to uppercase
            if (!String.IsNullOrEmpty(defaultDomain))
            {
                _defaultDomain = defaultDomain.ToUpperInvariant() + "\\";
            }
        }
        
        /// <summary>
        /// Gets the names of the groups a user is a member of.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<string> GetGroupNames()
        {
            var groupNames = new List<string>();
            if (_userIdentity != null)
            {
                foreach (IdentityReference group in _userIdentity.Groups)
                {
                    var groupName = group.Translate(typeof (NTAccount)).ToString().ToUpper(CultureInfo.CurrentCulture);
                    groupNames.Add(groupName);
                }
            }
            return groupNames;
        }

        /// <summary>
        /// Tests whether the user is in the specified security group
        /// </summary>
        /// <param name="groupName">The group name</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public bool UserIsInGroup(string groupName)
        {
            var groupList = new List<string> { groupName };
            return UserIsInGroup(groupList);
        }

        /// <summary>
        /// Tests whether the user is in one of the specified security groups
        /// </summary>
        /// <param name="groupNames">The group names.</param>
        /// <exception cref="System.ArgumentNullException">groupMembershipProvider</exception>
        /// <returns></returns>
        public bool UserIsInGroup(IList<string> groupNames)
        {
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

            var groupsForUser = GetGroupNames();

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
                    foreach (string group in groupNames)
                    {
                        var isInRole = UserIsInGroup(group);
                        groupMembershipCollection.Add(group, isInRole);
                    }
                }
            }

            return groupMembershipCollection;
        }
    }
}
