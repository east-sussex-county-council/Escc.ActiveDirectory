using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;

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
        /// Gets the LDAP path to be used when querying the <see cref="DefaultDomain"/>.
        /// </summary>
        /// <value>
        /// The LDAP path, eg LDAP://hostname.
        /// </value>
        public string LdapPath
        {
            get
            {
                return LdapPathForDomain(DefaultDomain);
            }
        }

        /// <summary>
        /// Gets the LDAP path to be used when querying a specific user, based on the domain they belong to.
        /// </summary>
        /// <param name="username">The username, including the user's domain.</param>
        /// <exception cref="System.ArgumentException">username</exception>
        /// <exception cref="System.FormatException">username</exception>
        /// <returns>
        /// The LDAP path, eg LDAP://hostname.
        /// </returns>
        public string LdapPathForUser(string username)
        {
            if (string.IsNullOrEmpty(username)) throw new ArgumentException(nameof(username));
            string domain = ExtractDomainFromUsername(username);
            return LdapPathForDomain(domain);
        }

        /// <summary>
        /// Gets the LDAP path to be used when querying a specific domain.
        /// </summary>
        /// <param name="domain">The domain.</param>
        /// <returns>
        /// The LDAP path, eg LDAP://hostname.
        /// </returns>
        public string LdapPathForDomain(string domain)
        {
            if (_generalSettings != null)
            {
                var value = _generalSettings["LdapPath." + domain];
                if (String.IsNullOrEmpty(value) && (String.IsNullOrEmpty(domain) || domain.ToUpperInvariant() == DefaultDomain?.ToUpperInvariant()))
                {
                    value = _generalSettings["LdapPath"];
                }
                return value;

            }
            return String.Empty;
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
                return LdapUsernameForDomain(DefaultDomain);   
            }
        }

        /// <summary>
        /// Gets the username of the account to be used when using LDAP queries when querying a specific domain.
        /// </summary>
        /// <returns>
        /// The username.
        /// </returns>
        public string LdapUsernameForDomain(string domain)
        {
            if (_generalSettings != null)
            {
                var value = _generalSettings["LdapUser." + domain];
                if (String.IsNullOrEmpty(value) && (String.IsNullOrEmpty(domain) || domain.ToUpperInvariant() == DefaultDomain?.ToUpperInvariant()))
                {
                    value = _generalSettings["LdapUser"];
                }
                return value;
            }
            return String.Empty;
        }

        /// <summary>
        /// Gets the username of the account to be used when using LDAP queries for a specific user, based on the domain they belong to.
        /// </summary>
        /// <param name="username">The username, including the user's domain.</param>
        /// <returns>The username.</returns>
        /// <exception cref="System.ArgumentException">username</exception>
        /// <exception cref="System.FormatException">username</exception>
        public string LdapUsernameForUser(string username)
        {
            if (string.IsNullOrEmpty(username)) throw new ArgumentException(nameof(username));
            string domain = ExtractDomainFromUsername(username);
            return LdapUsernameForDomain(domain);
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
                return LdapPasswordForDomain(DefaultDomain);
            }
        }

        /// <summary>
        /// Gets the password of the account to be used when using LDAP queries when querying a specific domain.
        /// </summary>
        /// <returns>
        /// The password.
        /// </returns>
        public string LdapPasswordForDomain(string domain)
        {
            if (_generalSettings != null)
            {
                var value = _generalSettings["LdapPassword." + domain];
                if (String.IsNullOrEmpty(value) && (String.IsNullOrEmpty(domain) || domain.ToUpperInvariant() == DefaultDomain?.ToUpperInvariant()))
                {
                    value = _generalSettings["LdapPassword"];
                }
                return value;
            }
            return String.Empty;
        }

        /// <summary>
        /// Gets the password of the account to be used when using LDAP queries for a specific user, based on the domain they belong to.
        /// </summary>
        /// <param name="username">The username, including the user's domain.</param>
        /// <returns>The password</returns>
        /// <exception cref="System.ArgumentException">username</exception>
        /// <exception cref="System.FormatException">username</exception>
        public string LdapPasswordForUser(string username)
        {
            if (string.IsNullOrEmpty(username)) throw new ArgumentException(nameof(username));
            string domain = ExtractDomainFromUsername(username);
            return LdapPasswordForDomain(domain);
        }

        private static string ExtractDomainFromUsername(string username)
        {
            string[] split = username.Split('\\');
            if (split.Length != 2) throw new FormatException($@"{nameof(username)} must be in the format domain\user. {nameof(username)} was {username}");
            string domain = split[0];
            return domain;
        }
    }
}
