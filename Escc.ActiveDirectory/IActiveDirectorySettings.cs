namespace Escc.ActiveDirectory
{
    /// <summary>
    /// Settings to be used when querying Active Directory
    /// </summary>
    public interface IActiveDirectorySettings
    {
        /// <summary>
        /// Gets the default domain to be assumed when querying.
        /// </summary>
        /// <value>
        /// The default domain.
        /// </value>
        string DefaultDomain { get; }

        /// <summary>
        /// Gets the LDAP path to be used when querying.
        /// </summary>
        /// <value>
        /// The LDAP path, eg LDAP://hostname.
        /// </value>
        string LdapPath { get; }

        /// <summary>
        /// Gets the username of the account to be used when using LDAP queries.
        /// </summary>
        /// <value>
        /// The username.
        /// </value>
        string LdapUsername { get; }

        /// <summary>
        /// Gets the password of the account to be used when using LDAP queries.
        /// </summary>
        /// <value>
        /// The password.
        /// </value>
        string LdapPassword { get; }
    }
}