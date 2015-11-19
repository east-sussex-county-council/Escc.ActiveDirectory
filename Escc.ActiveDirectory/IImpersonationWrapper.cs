using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Escc.ActiveDirectory
{
    /// <summary>
    /// A way to temporarily impersonate an Active Directory user
    /// </summary>
    interface IImpersonationWrapper
    {
        /// <summary>
        /// Impersonates an Active Directory user.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="domain">The domain.</param>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        bool ImpersonateUser(string username, string domain, string password);

        /// <summary>
        /// Ends impersonation begun using <see cref="ImpersonateUser"/>.
        /// </summary>
        void UndoUserImpersonation();
    }
}
