using System;
using System.Collections.ObjectModel;

namespace Escc.ActiveDirectory
{
    /// <summary>
    /// Custom Event argument for UsersFound event holding an ADUserCollection
    /// </summary>
    public class UsersFoundEventArgs : EventArgs
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
        /// Event arguments for <c>UsersFound</c> event
        /// </summary>
        /// <param name="userCollection">The collection containing one or more user objects representing the AD users found by search term</param>
        public UsersFoundEventArgs(Collection<ActiveDirectoryUser> userCollection)
        {
            this.userCollection = userCollection;
        }
        #endregion
    }
}