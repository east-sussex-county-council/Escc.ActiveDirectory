using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.DirectoryServices;
using System.Globalization;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;

namespace Escc.ActiveDirectory
{
    /// <summary>
    /// Search Active Directory for user and role membership data using LDAP
    /// </summary>
    public class LdapActiveDirectorySearcher : IActiveDirectorySearcher
    {
        #region private fields
        /// <summary>
        /// AD password stored in web.config
        /// </summary>
        private readonly string _adPassword;
        private readonly IActiveDirectoryCache _cache;

        /// <summary>
        /// AD user name stored in web.config
        /// </summary>
        private readonly string _adUser;
        /// <summary>
        /// AD server path stored in web.config
        /// </summary>
        private readonly string _ldapPath;
        /// <summary>
        /// private field to store user details
        /// </summary>
        private Collection<ActiveDirectoryUser> _userCollection;
        /// <summary>
        /// private field to store group details
        /// </summary>
        private Collection<ActiveDirectoryGroup> _groupsCollection;
        /// <summary>
        /// private field to store group names only
        /// </summary>
        private Collection<string> _groupNames;
        /// <summary>
        /// flag used to influence search filter choice
        /// </summary>
        private bool _searchBylogonFlag;
        /// <summary>
        /// flag used to influence search filter choice
        /// </summary>
        private bool _searchByGroupNameFlag;
        /// <summary>
        /// member to hold current culture info
        /// </summary>
        private readonly CultureInfo _culture = CultureInfo.CurrentCulture;

        /// <summary>
        /// Gets or sets the maximum number of results to return (to speed up large queries)
        /// </summary>
        public int MaximumResults { get; set; }
        #endregion

        #region constructors, destructors and initialisers

        /// <summary>
        /// Initializes a new instance of the <see cref="LdapActiveDirectorySearcher" /> class.
        /// </summary>
        /// <param name="ldapPath">The LDAP path.</param>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <param name="cache">The cache.</param>
        public LdapActiveDirectorySearcher(string ldapPath, string username, string password, IActiveDirectoryCache cache=null)
        {
            _adUser = username;
            _adPassword = password;
            _ldapPath = ldapPath;
            _cache = cache;
        }
        #endregion

        #region public methods
        /// <summary>
        /// Gets an Active Directory user based on logon name.
        /// </summary>
        /// <param name="accountName">user logon name</param>
        /// <param name="propertiesToLoad">Restricts the properties to load when searching for AD users. If collection is empty, all properties are loaded.</param>
        /// <returns>A single ActiveDirectoryUser object containing most properties associated with an AD user object, or <c>null</c> if not found.</returns>
        public ActiveDirectoryUser GetUserBySamAccountName(string accountName, IList<string> propertiesToLoad)
        {
            accountName = StripDomainFromSearchTerm(accountName);
            _searchBylogonFlag = true;
            SearchForUsers(accountName, propertiesToLoad);
            _searchBylogonFlag = false;

            foreach (ActiveDirectoryUser user in this._userCollection)
            {
                if (user.SamAccountName != null)
                {
                    string logonName = user.SamAccountName;
                    if (string.Compare(accountName, logonName, true, _culture) == 0)
                    {
                        OnUserFound();
                        return user;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Searches AD based on a partial username. If you have a full username use <seealso cref="GetUserBySamAccountName" /> instead.
        /// </summary>
        /// <param name="searchText">Part of an AD username</param>
        /// <param name="propertiesToLoad">Restricts the properties to load when searching for AD users. If collection is empty, all properties are loaded.</param>
        /// <returns>
        /// A collection of users with matching account names
        /// </returns>
        public Collection<ActiveDirectoryUser> SearchForUsersBySamAccountName(string searchText, IList<string> propertiesToLoad)
        {
            this._searchBylogonFlag = true;
            var users = this.SearchForUsers(searchText, propertiesToLoad);
            this._searchBylogonFlag = false;
            return users;
        }

        /// <summary>
        /// Performs a search for AD users using ambiguous name resolution (i.e. searches can be done using partial names).
        /// </summary>
        /// <param name="searchText">The search text.</param>
        /// <param name="propertiesToLoad">Restricts the properties to load when searching for AD users. If collection is empty, all properties are loaded.</param>
        /// <returns>
        /// An collection containing multiple ActiveDirectoryUser objects.
        /// </returns>
        public Collection<ActiveDirectoryUser> SearchForUsers(string searchText, IList<string> propertiesToLoad)
        {
            this._userCollection = null;

            searchText = StripDomainFromSearchTerm(searchText);

            string filter;
            if (_searchBylogonFlag)
            {
                filter = "(&(saMAccountName=" + searchText + ")(objectClass=user)(objectCategory=Person))";
            }
            else
            {
                filter = "(&(anr=" + searchText + ")(objectClass=user)(objectCategory=Person))";
            }

            string[] propertyNames = null;
            if (propertiesToLoad != null && propertiesToLoad.Count > 0)
            {
                propertyNames = new string[propertiesToLoad.Count];
                propertiesToLoad.CopyTo(propertyNames, 0);
            }

            // Combine the parameters into a cache key and check the cache
            var cacheKey = HashKeyForCache(_ldapPath + filter + (propertyNames != null ? String.Join(String.Empty, propertyNames) : String.Empty) + MaximumResults);
            if (_cache != null)
            {
                _userCollection = _cache.CheckForSavedValue<Collection<ActiveDirectoryUser>>(cacheKey);
            }

            if (_userCollection == null)
            {
                // Get results from Active Directory and save them to the cache
                _userCollection = new Collection<ActiveDirectoryUser>();

                using (var ent = new DirectoryEntry(_ldapPath))
                {
                    ent.Username = this._adUser;
                    ent.Password = this._adPassword;

                    using (DirectorySearcher ds = new DirectorySearcher())
                    {
                        ds.SearchRoot = ent;

                        ds.Filter = filter;

                        // If possible, restrict the properties to load to make the query faster
                        if (propertyNames != null) ds.PropertiesToLoad.AddRange(propertyNames);

                        // Restrict the size too, again to make it faster
                        if (this.MaximumResults > 0) ds.SizeLimit = this.MaximumResults;

                        SearchResultCollection results = ds.FindAll();

                        if (results.Count > 0)
                        {
                            if (propertyNames != null && propertyNames.Length > 0)
                            {
                                CreateUserCollection(results, propertyNames);
                            }
                            else
                            {
                                CreateUserCollectionAllProperties(results);
                            }
                        }
                    }
                }

                if (_cache != null)
                {
                    _cache.SaveValue(cacheKey, _userCollection);
                }
            }

            // Fire events
            if (_userCollection.Count > 1)
            {
                OnUsersFound();
            }
            if (_userCollection.Count == 1)
            {
                OnUserFound();
            }
            return this._userCollection;
        }

        /// <summary>
        /// The domain isn't required so be helpful and remove it if present, rather than returning no results
        /// </summary>
        /// <param name="searchText">The search text.</param>
        /// <returns></returns>
        private static string StripDomainFromSearchTerm(string searchText)
        {
            var domainSeparator = searchText.IndexOf("\\");
            if (domainSeparator > 0) { searchText = searchText.Substring(domainSeparator + 1); }

            return searchText;
        }

        /// <summary>
        /// Gets an AD group object.
        /// </summary>
        /// <param name="groupName">string. The name of the group to retrieve</param>
        /// <returns>A single ActiveDirectoryGroup containing ActiveDirectoryGroupMember objects, or <c>null</c> if not found. </returns>
        public ActiveDirectoryGroup GetGroupByGroupName(string groupName)
        {
            _searchByGroupNameFlag = true;
            SearchForGroups(groupName);
            _searchByGroupNameFlag = false;
            foreach (ActiveDirectoryGroup group in this._groupsCollection)
            {
                foreach (ActiveDirectoryGroupMember member in group)
                {
                    if (member.GroupName == groupName)
                    {
                        OnGroupFound();
                        return group;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Finds groups based on ambiguous name resolution
        /// </summary>
        /// <param name="searchText">string</param>
        /// <returns>An collection containing ActiveDirectoryGroup objects</returns>
        public Collection<ActiveDirectoryGroup> SearchForGroups(string searchText)
        {
            this._groupsCollection = null;

            string filter;
            if (_searchByGroupNameFlag)
            {
                filter = "(&(CN=" + searchText + ")(objectClass=group))";
            }
            else
            {
                filter = "(&(anr=" + searchText + ")(objectClass=group))";
            }

            // Combine the parameters into a cache key and check the cache
            var cacheKey = HashKeyForCache(_ldapPath + filter);
            if (_cache != null)
            {
                _groupsCollection = _cache.CheckForSavedValue<Collection<ActiveDirectoryGroup>>(cacheKey);
            }

            if (_groupsCollection == null)
            {
                // Get results from Active Directory and save them to the cache
                _groupsCollection = new Collection<ActiveDirectoryGroup>();

                using (DirectoryEntry ent = new DirectoryEntry(_ldapPath))
                {
                    using (DirectorySearcher ds = new DirectorySearcher())
                    {
                        ds.SearchRoot = ent;
                        ds.Filter = filter;

                        SearchResultCollection results = ds.FindAll();

                        if (results.Count > 0)
                        {
                            CreateGroupCollection(results);
                        }
                    }
                }

                if (_cache != null)
                {
                    _cache.SaveValue(cacheKey, _groupsCollection);
                }
            }

            // Fire events
            if (_groupsCollection.Count > 1)
            {
                OnGroupsFound();
            }
            if (_groupsCollection.Count == 1)
            {
                OnGroupFound();
            }
            return this._groupsCollection;
        }

        /// <summary>
        /// Finds group names based on ambiguous name resolution
        /// </summary>
        /// <param name="searchText">Group name with or without wildcard (e.g. "SOMEGROUP_*")</param>
        /// <returns>Group names as strings</returns>
        public Collection<string> GetGroupNames(string searchText)
        {
            _groupNames = null;
            using (DirectoryEntry ent = new DirectoryEntry(_ldapPath))
            {
                using (DirectorySearcher ds = new DirectorySearcher())
                {
                    ds.SearchRoot = ent;
                    ds.Filter = "(&(anr=" + searchText + ")(objectClass=group))";

                    // Combine the parameters into a cache key and check the cache
                    var cacheKey = HashKeyForCache("GetGroupNames" + _ldapPath + ds.Filter);
                    if (_cache != null)
                    {
                        _groupNames = _cache.CheckForSavedValue<Collection<string>>(cacheKey);
                    }

                    if (_groupNames == null)
                    {
                        _groupNames = new Collection<string>();

                        SearchResultCollection src = ds.FindAll();

                        foreach (SearchResult res in src)
                        {
                            string path = res.Path;
                            path = path.Remove(0, 28);
                            path = path.Replace("CN=", "");
                            path = path.Remove(path.IndexOf(","), path.Length - path.IndexOf(","));
                            _groupNames.Add(path);
                        }

                        if (_cache != null)
                        {
                            _cache.SaveValue(cacheKey, _groupNames);
                        }
                    }
                }
            }
            return this._groupNames;
        }
        /// <summary>
        /// Finds group paths based on ambiguous name resolution
        /// </summary>
        /// <param name="searchText">Group name with or without wildcard (e.g. "SOMEGROUP_*")</param>
        /// <returns>Group paths as strings</returns>
        public Collection<string> GetGroupPaths(string searchText)
        {
            _groupNames = null;
            using (DirectoryEntry ent = new DirectoryEntry(_ldapPath))
            {
                using (DirectorySearcher ds = new DirectorySearcher())
                {
                    ds.SearchRoot = ent;
                    ds.Filter = "(&(anr=" + searchText + ")(objectClass=group))";

                    // Combine the parameters into a cache key and check the cache
                    var cacheKey = HashKeyForCache("GetGroupPaths" + _ldapPath + ds.Filter);
                    if (_cache != null)
                    {
                        _groupNames = _cache.CheckForSavedValue<Collection<string>>(cacheKey);
                    }

                    if (_groupNames == null)
                    {
                        _groupNames = new Collection<string>();

                        SearchResultCollection src = ds.FindAll();

                        foreach (SearchResult res in src)
                        {
                            _groupNames.Add(res.Path);
                        }

                        if (_cache != null)
                        {
                            _cache.SaveValue(cacheKey, _groupNames);
                        }
                    }
                }
            }
            return this._groupNames;
        }
        #endregion
        
        #region private methods
        /// <summary>
        /// Helper method populates a collection of groups.
        /// </summary>
        /// <param name="results">System.DirectoryServices.SearchResultCollection</param>
        private void CreateGroupCollection(SearchResultCollection results)
        {
            string groupName = null;
            string samAccountName = null;
            foreach (SearchResult groupObject in results)
            {
                ActiveDirectoryGroup group = new ActiveDirectoryGroup();
                PropertyCollection propcoll = groupObject.GetDirectoryEntry().Properties;
                foreach (string key in groupObject.GetDirectoryEntry().Properties.PropertyNames)
                {
                    //loop through all the values associated with our key
                    foreach (object values in propcoll[key])
                    {
                        // get the group name of this entry
                        if (string.Compare(key, "cn", true, _culture) == 0)
                        {
                            groupName = values.ToString();
                        }
                        // get the samAccountName name of this entry
                        if (string.Compare(key, "samAccountName", true, _culture) == 0)
                        {
                            samAccountName = values.ToString();
                        }
                        // get the member name of this entry
                        if (string.Compare(key, "member", true, _culture) == 0)
                        {
                            ActiveDirectoryGroupMember member = new ActiveDirectoryGroupMember();
                            member.GroupName = groupName;
                            member.GroupMember = ParseString(values);
                            member.MemberString = values.ToString();
                            group.Add(member);
                        }
                    }
                }
                if (group != null)
                {
                    _groupsCollection.Add(group);
                }
            }
        }

        /// <summary>
        /// Helper method populates collection with ActiveDirectoryUser objects.
        /// </summary>
        /// <param name="results">System.DirectoryServices.SearchResultCollection</param>
        /// <param name="propertyNames">The property names to load.</param>
        private void CreateUserCollection(SearchResultCollection results, string[] propertyNames)
        {
            if (propertyNames != null && propertyNames.Length > 0)
            {
                foreach (SearchResult r in results)
                {
                    PropertyCollection propcoll = r.GetDirectoryEntry().Properties;
                    ActiveDirectoryUser user = new ActiveDirectoryUser();

                    string[] propertyNamesForResult = propertyNames;
                    if (propertyNamesForResult == null || propertyNamesForResult.Length == 0)
                    {
                        propertyNamesForResult = new string[propcoll.Count];
                        propcoll.PropertyNames.CopyTo(propertyNamesForResult, 0);
                    }

                    //loop through all of the properties for this record
                    foreach (string key in propertyNamesForResult)
                    {
                        // Faster code to read property values. This could replace the old code in CreateUserCollectionAllProperties
                        // if (1) every property always has a PropertyValueCollection (2) the lowercase AD property names
                        // can be made to relate to the mixed case object property names. Not all scenarios tested so old
                        // still available.
                        PropertyValueCollection propertyValues = propcoll[key];
                        if (propertyValues != null && propertyValues.Count > 0)
                        {
                            // AD potentially holds multiple values for a property, but the ActiveDirectoryUser object only 
                            // supports one for each, so just use the first value. CreateUserCollectionAllProperties just used
                            // the last value because it overwrites any previous one found.
                            PropertyInfo userProperty = user.GetType().GetProperty(key, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                            if (userProperty != null) userProperty.SetValue(user, ParseString(propertyValues[0]), null);
                        }
                    }
                    _userCollection.Add(user);
                }
            }
        }

        /// <summary>
        /// Helper method populates collection with ActiveDirectoryUser objects.
        /// </summary>
        /// <param name="results">System.DirectoryServices.SearchResultCollection</param>
        private void CreateUserCollectionAllProperties(SearchResultCollection results)
        {
            foreach (SearchResult r in results)
            {
                PropertyCollection propcoll = r.GetDirectoryEntry().Properties;
                ActiveDirectoryUser user = new ActiveDirectoryUser();

                var propertyNamesForResult = new string[propcoll.Count];
                propcoll.PropertyNames.CopyTo(propertyNamesForResult, 0);

                //loop through all of the properties for this record
                foreach (string key in propertyNamesForResult)
                {
                    // Slow approach used for generic queries that don't specify properties to load (effectively SELECT * queries). 
                    // Loop through all the values associated with our key, testing every possible key they might match.
                    foreach (object values in propcoll[key])
                    {
                        switch (key.ToLower())
                        {
                            case "title":
                                // get the title of this entry 
                                user.Title = ParseString(values);
                                break;
                            case "sn":
                                // get the sn of this entry 
                                user.SN = ParseString(values);
                                break;
                            case "distinguishedname":
                                // get the distinguished of this entry 
                                user.DistinguishedName = ParseString(values);
                                break;
                            case "name":
                                // get the name of this entry 
                                user.Name = ParseString(values);
                                break;
                            case "givenname":
                                // get the given name of this entry 
                                user.GivenName = ParseString(values);
                                break;
                            case "displayname":
                                // get the display name of this entry 
                                user.DisplayName = ParseString(values);
                                break;
                            case "mail":
                                // get the mail of this entry 
                                user.Mail = ParseString(values);
                                break;
                            case "targetaddress":
                                // get the targetaddress of this entry 
                                user.TargetAddress = ParseString(values);
                                break;
                            case "samaccountname":
                                // get the sAMAccountName of this entry 
                                user.SamAccountName = ParseString(values);
                                break;
                            case "physicaldeliveryofficename":
                                // get the office address of this entry 
                                user.PhysicalDeliveryOfficeName = ParseString(values);
                                break;
                            case "telephonenumber":
                                // get the telephone number of this entry 
                                user.TelephoneNumber = ParseString(values);
                                break;
                            case "department":
                                // get the department of this entry 
                                user.Department = ParseString(values);
                                break;
                            case "userprincipalname":
                                // get the principal of this entry 
                                user.UserPrincipalName = ParseString(values);
                                break;
                            case "memberof":
                                // get the permission groups of this entry 
                                user.MemberOf = ParseString(values);
                                break;
                            case "description":
                                // get the description of this entry 
                                user.Description = ParseString(values);
                                break;
                            case "company":
                                // get the company of this entry 
                                user.Company = ParseString(values);
                                break;
                            case "streetaddress":
                                // get the street address of this entry 
                                user.StreetAddress = ParseString(values);
                                break;
                            case "postalcode":
                                // get the postal code of this entry 
                                user.PostalCode = ParseString(values);
                                break;
                            case "manager":
                                // get the manager of this entry 
                                user.Manager = ParseString(values);
                                break;
                            case "st":
                                // get the st of this entry 
                                user.ST = ParseString(values);
                                break;
                            case "mobile":
                                // get the mobile of this entry 
                                user.Mobile = ParseString(values);
                                break;
                            case "homephone":
                                // get the home phone of this entry 
                                user.HomePhone = ParseString(values);
                                break;
                            case "l":
                                // get the L of this entry 
                                user.L = ParseString(values);
                                break;
                            case "location":
                                // get the Location of this entry 
                                user.Location = ParseString(values);
                                break;
                            case "c":
                                // get the c of this entry 
                                user.C = ParseString(values);
                                break;
                            case "cn":
                                // get the cn of this entry 
                                user.CN = ParseString(values);
                                break;
                            case "whencreated":
                                // get the when created of this entry 
                                user.WhenCreated = ParseString(values);
                                break;
                            default:
                                break;
                        }
                    }
                }
                _userCollection.Add(user);
            }
        }

        private string HashKeyForCache(string text)
        {
            // MD5 for speed - it just needs to be unique, not secure
            StringBuilder sb = new StringBuilder();
            using (var md5 = MD5.Create())
            {
                var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(text));
                foreach (var hashByte in hash)
                {
                    sb.Append(hashByte.ToString("x2"));
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Strips the cn value from a string beginning CN=value.
        /// </summary>
        /// <param name="values">An object containing string values.</param>
        /// <returns>A string</returns>
        private static string ParseString(object values)
        {
            string temp = values.ToString();
            string tmp = null;
            if (temp.IndexOf("ImportedExchange") < 0)
            {
                string delimStr = ",";
                char[] delimiter = delimStr.ToCharArray();
                string[] atemp = temp.Split(delimiter);
                tmp = atemp[0].Replace("CN=", "");
            }
            return tmp;
        }
        #endregion
        
        #region events and delegates
        /// <summary>
        /// Event indicating that a group has been found by groupname
        /// </summary>
        public event GroupFoundEventHandler GroupFound;

        /// <summary>
        /// 
        /// </summary>
        protected void OnGroupFound()
        {
            // check there are handlers for the event before raising
            if (this.GroupFound != null)
            {
                // raise event
                this.GroupFound(this, new GroupFoundEventArgs(this._groupsCollection));
            }
        }
        /// <summary>
        /// Event indicating that multiple groups have been found by search term
        /// </summary>
        public event GroupsFoundEventHandler GroupsFound;

        protected void OnGroupsFound()
        {
            // check there are handlers for the event before raising
            if (this.GroupsFound != null)
            {
                // raise event
                this.GroupsFound(this, new GroupsFoundEventArgs(this._groupsCollection));
            }
        }
        /// <summary>
        /// Event indicating that a user has been found by user principal name
        /// </summary>
        public event UserFoundEventHandler UserFound;
        protected void OnUserFound()
        {
            // check there are handlers for the event before raising
            if (this.UserFound != null)
            {
                // raise event
                this.UserFound(this, new UserFoundEventArgs(this._userCollection));
            }
        }
        /// <summary>
        /// Event indicating that a user or users have has been found by search term
        /// </summary>
        public event UsersFoundEventHandler UsersFound;
        protected void OnUsersFound()
        {
            // check there are handlers for the event before raising
            if (this.UsersFound != null)
            {
                // raise event
                this.UsersFound(this, new UsersFoundEventArgs(this._userCollection));
            }
        }
        #endregion
    }
}

