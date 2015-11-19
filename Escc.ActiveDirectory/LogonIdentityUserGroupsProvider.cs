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
    public class LogonIdentityUserGroupsProvider : IUserGroupsProvider
    {
        /// <summary>
        /// Gets the names of the groups a user is a member of.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetGroupNames()
        {
            var groupNames = new List<string>();
            foreach (IdentityReference group in HttpContext.Current.Request.LogonUserIdentity.Groups)
            {
                var groupName = group.Translate(typeof (NTAccount)).ToString().ToUpper(CultureInfo.CurrentCulture);
                groupNames.Add(groupName);
            }
            return groupNames;
        }
    }
}
