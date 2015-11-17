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
    /// Checks the permissions of the current user of a web application
    /// </summary>
    public static class WebUserPermissions
    {
        /// <summary>
        /// Saves the result of a check for whether a user is in a set of security groups.
        /// </summary>
        /// <param name="groupsToMatch">The groups to match.</param>
        /// <param name="result"><c>true</c> if the user was in the group; <c>false</c> otherwise</param>
        private static void SaveGroupMatchResult(string[] groupsToMatch, bool result)
        {
            // Important to save in session rather than an instance variable, to minimise security checks
            // from multiple instances of the class.
            var sess = HttpContext.Current.Session;
            if (sess == null) return;

            Dictionary<int, bool> groupMatchResults = null;
            if (sess["GroupMatchResults"] != null) groupMatchResults = sess["GroupMatchResults"] as Dictionary<int, bool>;
            if (groupMatchResults == null) groupMatchResults = new Dictionary<int, bool>();

            var key = String.Join(String.Empty, groupsToMatch).ToUpperInvariant().GetHashCode();
            if (!groupMatchResults.ContainsKey(key)) groupMatchResults.Add(key, result);

            sess["GroupMatchResults"] = groupMatchResults;
        }

        /// <summary>
        /// Gets the result of a security group check if it's already been done in the current session.
        /// </summary>
        /// <param name="groupsToMatch">The groups to match.</param>
        /// <returns></returns>
        private static bool? CheckGroupMatchResult(string[] groupsToMatch)
        {
            // Important to use session to cache this, because security checks can take a while so we need
            // to minimise the number of times they're done, even across instances. When using inside a loop, 
            // there can be a *lot* of instances.
            var sess = HttpContext.Current.Session;
            if (sess == null || sess["GroupMatchResults"] == null) return null;

            var groupMatchResults = sess["GroupMatchResults"] as Dictionary<int, bool>;
            var key = String.Join(String.Empty, groupsToMatch).ToUpperInvariant().GetHashCode();
            if (!groupMatchResults.ContainsKey(key)) return null;

            return groupMatchResults[key];
        }

        /// <summary>
        /// Tests whether the user is in one of the specified security groups
        /// </summary>
        /// <param name="groupNames">The group names, separated by semi-colons.</param>
        /// <returns></returns>
        public static bool UserIsInGroup(string groupNames)
        {
            if (String.IsNullOrEmpty(groupNames)) throw new ArgumentNullException("groupNames", "groupNames cannot be null or an empty string");
            return UserIsInGroup(groupNames.Split(';'));
        }

        /// <summary>
        /// Tests whether the user is in one of the specified security groups
        /// </summary>
        /// <param name="groupNames">The group names.</param>
        /// <returns></returns>
        public static bool UserIsInGroup(string[] groupNames)
        {
            if (groupNames == null || groupNames.Length == 0) throw new ArgumentNullException("groupNames", "groupNames cannot be null or an empty array");

            // If we've already done the check, return the previous result
            var storedResult = CheckGroupMatchResult(groupNames);
            if (storedResult.HasValue) return storedResult.Value;

            // Normalise: Remove the default domain prefix if present, and convert to uppercase
            var defaultDomain = String.Empty;
            var config = ConfigurationManager.GetSection("EsccWebTeam.Data.ActiveDirectory/GeneralSettings") as NameValueCollection;
            if (config != null)
            {
                defaultDomain = config["DefaultDomain"].ToUpperInvariant() + "\\";
            }

            var len = groupNames.Length;
            for (var i = 0; i < len; i++)
            {
                groupNames[i] = groupNames[i].ToUpper(CultureInfo.CurrentCulture);
                if (!String.IsNullOrEmpty(defaultDomain) && groupNames[i].StartsWith(defaultDomain, StringComparison.Ordinal))
                {
                    groupNames[i] = groupNames[i].Substring(defaultDomain.Length);
                }
            }

            // Add to List<string> to get access to .Contains method
            var groupsList = new List<string>(groupNames);

            foreach (IdentityReference group in HttpContext.Current.Request.LogonUserIdentity.Groups)
            {
                var groupName = group.Translate(typeof(NTAccount)).ToString().ToUpper(CultureInfo.CurrentCulture);
                if (!String.IsNullOrEmpty(defaultDomain) && groupName.StartsWith(defaultDomain, StringComparison.Ordinal))
                {
                    groupName = groupName.Substring(defaultDomain.Length);
                }
                if (groupsList.Contains(groupName))
                {
                    // Stash the result so we don't have to do this check again
                    SaveGroupMatchResult(groupNames, true);
                    return true;
                }
            }

            SaveGroupMatchResult(groupNames, false);
            return false;
        }
    }
}
