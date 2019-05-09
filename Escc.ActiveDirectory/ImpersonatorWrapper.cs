using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Web.Compilation;

namespace Escc.ActiveDirectory
{
    /// <summary>
    /// Temporarily impersonate an Active Directory user using the <see cref="Impersonator"/> class.
    /// </summary>
    /// <remarks>This class allows <see cref="IImpersonationWrapper"/> to be injected as a dependency where required, 
    /// without making <see cref="Impersonator"/> non-static. It's not clear whether it is required to be static to work.</remarks>
    public class ImpersonatorWrapper : IImpersonationWrapper
    {
        /// <summary>
        /// Impersonates an Active Directory user.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="domain">The domain.</param>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        [SecuritySafeCritical]
        public bool ImpersonateUser(string username, string domain, string password)
        {
            return Impersonator.ImpersonateUser(username, domain, password);
        }

        /// <summary>
        /// Ends impersonation begun using <see cref="ImpersonateUser"/>.
        /// </summary>
        [SecuritySafeCritical]
        public void UndoUserImpersonation()
        {
            Impersonator.UndoUserImpersonation();
        }
    }
}
