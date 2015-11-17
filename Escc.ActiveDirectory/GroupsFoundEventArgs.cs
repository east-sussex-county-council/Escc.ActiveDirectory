using System;
using System.Collections.ObjectModel;

namespace Escc.ActiveDirectory
{
    /// <summary>
    /// Custom Event argument for GroupsFound event holding an ADGroupsCollection
    /// </summary>
    public class GroupsFoundEventArgs : EventArgs
    {
        #region Fields
        /// <summary>
        /// Store ADGroupsCollection
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
        /// <param name="groupsCollection">The collection containing one or more user objects representing the AD groups found by search term</param>
        public GroupsFoundEventArgs(Collection<ActiveDirectoryGroup> groupsCollection)
        {
            this.groupsCollection = groupsCollection;
        }
        #endregion
    }
}