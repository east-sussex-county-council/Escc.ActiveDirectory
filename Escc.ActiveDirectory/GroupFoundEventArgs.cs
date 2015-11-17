using System;
using System.Collections.ObjectModel;

namespace Escc.ActiveDirectory
{
    /// <summary>
    /// Custom Event argument for GroupFound event holding an ADGroupsCollection
    /// </summary>
    public class GroupFoundEventArgs : EventArgs
    {
        #region Fields
        /// <summary>
        /// Store ActiveDirectoryGroup
        /// </summary>
        private Collection<ActiveDirectoryGroup> groupsCollection;
        #endregion
        #region properties
        /// <summary>
        /// public read only property holding  an collection of groups
        /// </summary>
        public Collection<ActiveDirectoryGroup> GroupsCollection
        {
            get
            {
                return groupsCollection;
            }
        }
        #endregion
        #region Constructors
        /// <summary>
        /// Event arguments for <c>GroupsFound</c> event
        /// </summary>
        /// <param name="groupsCollection">The collection containing one or more AD groups found by search term</param>
        public GroupFoundEventArgs(Collection<ActiveDirectoryGroup> groupsCollection)
        {
            this.groupsCollection = groupsCollection;
        }
        #endregion
    }
}