using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Security.Principal;

namespace Escc.ActiveDirectory
{
    /// <summary>
    /// Search an instance of Active Directory for user and role membership data
    /// </summary>
    public interface IActiveDirectorySearcher
    {
        /// <summary>
        /// Gets a user based on account name.
        /// </summary>
        /// <param name="accountName">user logon name</param>
        /// <param name="propertiesToLoad">Restricts the properties to load when searching for users. If collection is empty, all properties are loaded.</param>
        /// <returns>A single ActiveDirectoryUser object containing most properties associated with a user, or <c>null</c> if not found.</returns>
        ActiveDirectoryUser GetUserBySamAccountName(string accountName, IList<string> propertiesToLoad);

        /// <summary>
        /// Searches AD based on a partial username. If you have a full username use <seealso cref="GetUserBySamAccountName" /> instead.
        /// </summary>
        /// <param name="searchText">Part of an AD username</param>
        /// <param name="propertiesToLoad">Restricts the properties to load when searching for AD users. If collection is empty, all properties are loaded.</param>
        /// <returns>
        /// A collection of users with matching account names
        /// </returns>
        Collection<ActiveDirectoryUser> SearchForUsersBySamAccountName(string searchText, IList<string> propertiesToLoad);

        /// <summary>
        /// Performs a search for AD users using ambiguous name resolution (i.e. searches can be done using partial names).
        /// </summary>
        /// <param name="searchText">The search text.</param>
        /// <param name="propertiesToLoad">Restricts the properties to load when searching for AD users. If collection is empty, all properties are loaded.</param>
        /// <returns>
        /// An collection containing multiple ActiveDirectoryUser objects.
        /// </returns>
        Collection<ActiveDirectoryUser> SearchForUsers(string searchText, IList<string> propertiesToLoad);

        /// <summary>
        /// Gets an AD group object.
        /// </summary>
        /// <param name="groupName">string. The name of the group to retrieve</param>
        /// <returns>A single ActiveDirectoryGroup containing ActiveDirectoryGroupMember objects, or <c>null</c> if not found. </returns>
        ActiveDirectoryGroup GetGroupByGroupName(string groupName);

        /// <summary>
        /// Finds groups based on ambiguous name resolution
        /// </summary>
        /// <param name="searchText">string</param>
        /// <returns>An collection containing ActiveDirectoryGroup objects</returns>
        Collection<ActiveDirectoryGroup> SearchForGroups(string searchText);

        /// <summary>
        /// Finds group names based on ambiguous name resolution
        /// </summary>
        /// <param name="searchText">Group name with or without wildcard (e.g. "SOMEGROUP_*")</param>
        /// <returns>Group names as strings</returns>
        Collection<string> GetGroupNames(string searchText);
        
        /// <summary>
        /// Finds group paths based on ambiguous name resolution
        /// </summary>
        /// <param name="searchText">Group name with or without wildcard (e.g. "SOMEGROUP_*")</param>
        /// <returns>Group paths as strings</returns>
        Collection<string> GetGroupPaths(string searchText);

        /// <summary>
        /// Event indicating that a group has been found by groupname
        /// </summary>
        event GroupFoundEventHandler GroupFound;

        /// <summary>
        /// Event indicating that multiple groups have been found by search term
        /// </summary>
        event GroupsFoundEventHandler GroupsFound;

        /// <summary>
        /// Event indicating that a user has been found by user principal name
        /// </summary>
        event UserFoundEventHandler UserFound;

        /// <summary>
        /// Event indicating that a user or users have has been found by search term
        /// </summary>
        event UsersFoundEventHandler UsersFound;
    }
}