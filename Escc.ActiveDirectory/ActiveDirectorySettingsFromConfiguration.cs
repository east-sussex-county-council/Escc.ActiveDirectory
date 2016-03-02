using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Text;

namespace Escc.ActiveDirectory
{
    /// <summary>
    /// Gets configuration settings from web.config or app.config
    /// </summary>
    public class ActiveDirectorySettingsFromConfiguration : IActiveDirectorySettings
    {
        private NameValueCollection _generalSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActiveDirectorySettingsFromConfiguration"/> class.
        /// </summary>
        public ActiveDirectorySettingsFromConfiguration()
        {
            _generalSettings = ConfigurationManager.GetSection("Escc.ActiveDirectory/GeneralSettings") as NameValueCollection;
        }

        /// <summary>
        /// Gets the default domain to be assumed when querying.
        /// </summary>
        /// <value>
        /// The default domain.
        /// </value>
        public string DefaultDomain
        {
            get
            {
                if (_generalSettings != null)
                {
                    return _generalSettings["DefaultDomain"];
                }
                return String.Empty;
            }
        }

        /// <summary>
        /// Gets the LDAP path to be used when querying.
        /// </summary>
        /// <value>
        /// The LDAP path, eg LDAP://hostname.
        /// </value>
        public string LdapPath
        {
            get
            {
                if (_generalSettings != null)
                {
                    return _generalSettings["LdapPath"];
                }
                return String.Empty;
            }
        }

        /// <summary>
        /// Gets the username of the account to be used when using LDAP queries.
        /// </summary>
        /// <value>
        /// The username.
        /// </value>
        public string LdapUsername
        {
            get
            {
                if (_generalSettings != null)
                {
                    return _generalSettings["LdapUser"];
                }
                return String.Empty;
            }
        }


        /// <summary>
        /// Gets the password of the account to be used when using LDAP queries.
        /// </summary>
        /// <value>
        /// The password.
        /// </value>
        public string LdapPassword
        {
            get
            {
                if (_generalSettings != null)
                {
                    return _generalSettings["LdapPassword"];
                }
                return String.Empty;
            }
        }
    }
}
