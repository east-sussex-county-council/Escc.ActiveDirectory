using System;
using System.Collections.Generic;
using System.Web;

namespace Escc.ActiveDirectory
{
    /// <summary>
    /// Caches the result of permissions queries in session state
    /// </summary>
    public class SessionPermissionsResultCache : IPermissionsResultCache
    {
        private const string SessionKey = "Escc.ActiveDirectory.GroupMatchResults";
        
        /// <summary>
        /// Saves the result of a check for whether a user is in a set of security groups.
        /// </summary>
        /// <param name="groupsToMatch">The groups to match.</param>
        /// <param name="result"><c>true</c> if the user was in the group; <c>false</c> otherwise</param>
        public void SaveGroupMatchResult(string[] groupsToMatch, bool result)
        {
            // Important to save in session rather than an instance variable, to minimise security checks
            // from multiple instances of the class.
            var sess = HttpContext.Current.Session;
            if (sess == null) return;

            Dictionary<int, bool> groupMatchResults = null;
            if (sess[SessionKey] != null) groupMatchResults = sess[SessionKey] as Dictionary<int, bool>;
            if (groupMatchResults == null) groupMatchResults = new Dictionary<int, bool>();

            var key = String.Join(String.Empty, groupsToMatch).ToUpperInvariant().GetHashCode();
            if (!groupMatchResults.ContainsKey(key)) groupMatchResults.Add(key, result);

            sess[SessionKey] = groupMatchResults;
        }

        /// <summary>
        /// Gets the result of a security group check if it's already been done in the current session.
        /// </summary>
        /// <param name="groupsToMatch">The groups to match.</param>
        /// <returns></returns>
        public bool? CheckGroupMatchResult(string[] groupsToMatch)
        {
            // Important to use session to cache this, because security checks can take a while so we need
            // to minimise the number of times they're done, even across instances. When using inside a loop, 
            // there can be a *lot* of instances.
            var sess = HttpContext.Current.Session;
            if (sess == null || sess[SessionKey] == null) return null;

            var groupMatchResults = sess[SessionKey] as Dictionary<int, bool>;
            var key = String.Join(String.Empty, groupsToMatch).ToUpperInvariant().GetHashCode();
            if (!groupMatchResults.ContainsKey(key)) return null;

            return groupMatchResults[key];
        }
    }
}