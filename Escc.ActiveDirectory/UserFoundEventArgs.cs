using System;
using System.Collections.ObjectModel;

namespace Escc.ActiveDirectory
{
    /// <summary>
    /// Custom Event argument for UserFound event holding an ADUserCollection
    /// </summary>
    public class UserFoundEventArgs : EventArgs
    {
        #region Fields
        /// <summary>
        /// Store ADUserCollection
        /// </summary>
        private Collection<ActiveDirectoryUser> userCollection;
        #endregion
        #region properties
        /// <summary>
        /// public read only property holding  an ActiveDirectoryUser collection
        /// </summary>
        public Collection<ActiveDirectoryUser> UserCollection
        {
            get
            {
                return userCollection;
            }
        }
        #endregion
        #region Constructors
        /// <summary>
        /// Event arguments for <c>UserFound</c> event
        /// </summary>
        /// <param name="userCollection">The collection containing one user object representing the AD user by principal name (logon name)</param>
        public UserFoundEventArgs(Collection<ActiveDirectoryUser> userCollection)
        {
            this.userCollection = userCollection;
        }
        #endregion
    }
}