using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Principal;
using System.Runtime.InteropServices;

namespace EsccWebTeam.Data.ActiveDirectory
{
    /// <summary>
    /// This class allows the user to impersonate a domain account to get round the double hop problem.
    /// The double-hop issue is when the ASPX page tries to use resources that are located on a server that is different from the IIS server
    /// that you are making the primary request to. Call the ImpersonateUser() method and pass in a set of credentials that have sufficent permissions on
    /// the file or folder you wish to access. After you have completed the operation call UndoUserImpersonation() method to return identity to original.
    /// 
    /// </summary>
    public class DoubleHop
    {

        private const int LOGON32_PROVIDER_DEFAULT = 0;
        private const int LOGON32_LOGON_INTERACTIVE = 2;
        private const int LOGON32_LOGON_NETWORK = 3;
        private const int LOGON32_LOGON_BATCH = 4;
        private const int LOGON32_LOGON_SERVICE = 5;
        private const int LOGON32_LOGON_UNLOCK = 7;
        private const int LOGON32_LOGON_NETWORK_CLEARTEXT = 8;
        private const int LOGON32_LOGON_NEW_CREDENTIALS = 9;
        private static WindowsImpersonationContext ImpersonationContext;

        [DllImport("advapi32.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int LogonUserA(string lpszUsername, string lpszDomain, string lpszPassword, int dwLogonType, int dwLogonProvider, ref IntPtr phToken);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern int DuplicateToken(IntPtr ExistingTokenHandle, int ImpersonationLevel, ref IntPtr DuplicateTokenHandle);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern long RevertToSelf();

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern long CloseHandle(IntPtr handle);

        public static bool ImpersonateUser(string usr, string domain, string pwd)
        {
            bool retval = false;
            IntPtr token = IntPtr.Zero;
            IntPtr tokenDuplicate = IntPtr.Zero;


            if (RevertToSelf() != 0)
            {
                if (LogonUserA(usr, domain, pwd, LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT, ref token) != 0)
                {
                    if (DuplicateToken(token, 2, ref tokenDuplicate) != 0)
                    {
                        WindowsIdentity wi = new WindowsIdentity(tokenDuplicate);
                        ImpersonationContext = wi.Impersonate();
                        if ((ImpersonationContext != null))
                        {
                            retval = true;
                        }
                    }
                }
            }
            if (!tokenDuplicate.Equals(IntPtr.Zero))
            {
                CloseHandle(tokenDuplicate);
            }
            if (!token.Equals(IntPtr.Zero))
            {
                CloseHandle(token);
            }
            return retval;
        }

        public static void UndoUserImpersonation()
        {
            ImpersonationContext.Undo();
        }
    }
}

