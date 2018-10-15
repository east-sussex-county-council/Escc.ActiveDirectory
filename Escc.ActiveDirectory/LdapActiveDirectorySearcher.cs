using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.DirectoryServices;
using System.Globalization;
using System.Reflection;
using System.Security.Principal;

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
        public LdapActiveDirectorySearcher(string ldapPath, string username, string password)
        {
            _adUser = username;
            _adPassword = password;
            _ldapPath = ldapPath;
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
            this._userCollection = new Collection<ActiveDirectoryUser>();
            using (var ent = new DirectoryEntry(_ldapPath))
            {
                ent.Username = this._adUser;
                ent.Password = this._adPassword;
                using (DirectorySearcher ds = new DirectorySearcher())
                {
                    ds.SearchRoot = ent;

                    searchText = StripDomainFromSearchTerm(searchText);

                    if (_searchBylogonFlag)
                    {
                        ds.Filter = "(&(saMAccountName=" + searchText + ")(objectClass=user)(objectCategory=Person))";
                    }
                    else
                    {
                        ds.Filter = "(&(anr=" + searchText + ")(objectClass=user)(objectCategory=Person))";
                    }

                    // If possible, restrict the properties to load to make the query faster
                    string[] propertyNames = null;

                    if (propertiesToLoad != null && propertiesToLoad.Count > 0)
                    {
                        propertyNames = new string[propertiesToLoad.Count];
                        propertiesToLoad.CopyTo(propertyNames, 0);
                        ds.PropertiesToLoad.AddRange(propertyNames);
                    }

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
                    if (results.Count > 1)
                    {
                        OnUsersFound();
                    }
                    if (results.Count == 1)
                    {
                        OnUserFound();
                    }
                }
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
            this._groupsCollection = new Collection<ActiveDirectoryGroup>();
            DirectoryEntry ent = new DirectoryEntry(_ldapPath);
            DirectorySearcher ds = new DirectorySearcher();
            ds.SearchRoot = ent;
            if (_searchByGroupNameFlag)
            {
                ds.Filter = "(&(CN=" + searchText + ")(objectClass=group))";
            }
            else
            {
                ds.Filter = "(&(anr=" + searchText + ")(objectClass=group))";
            }
            try
            {
                SearchResultCollection results = ds.FindAll();

                if (results.Count > 0)
                {
                    CreateGroupCollection(results);
                }
                if (results.Count > 1)
                {
                    OnGroupsFound();
                }
                if (results.Count == 1)
                {
                    OnGroupFound();
                }
            }
            finally
            {
                ds.Dispose();
                ent.Close();
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
            _groupNames = new Collection<string>();
            DirectoryEntry ent = new DirectoryEntry(_ldapPath);
            DirectorySearcher ds = new DirectorySearcher();
            ds.SearchRoot = ent;
            ds.Filter = "(&(anr=" + searchText + ")(objectClass=group))";
            SearchResultCollection src = ds.FindAll();
            foreach (SearchResult res in src)
            {
                string path = res.Path;
                path = path.Remove(0, 28);
                path = path.Replace("CN=", "");
                path = path.Remove(path.IndexOf(","), path.Length - path.IndexOf(","));
                _groupNames.Add(path);
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
            _groupNames = new Collection<string>();
            DirectoryEntry ent = new DirectoryEntry(_ldapPath);
            DirectorySearcher ds = new DirectorySearcher();
            ds.SearchRoot = ent;
            ds.Filter = "(&(anr=" + searchText + ")(objectClass=group))";
            SearchResultCollection src = ds.FindAll();
            foreach (SearchResult res in src)
            {
                _groupNames.Add(res.Path);
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
                        /*
                        // get the title of this entry 
                        if (string.Compare(key, "title", true, _culture) == 0)
                        {
                            user.Title = ParseString(values);
                        }
                        // get the title of this entry  
                        if (string.Compare(key, "sn", true, _culture) == 0)
                        {
                            user.SN = ParseString(values);
                        }
                        // get the distinguished name of this entry
                        if (string.Compare(key, "distinguishedname", true, _culture) == 0)
                        {
                            user.DistinguishedName = ParseString(values);
                        }
                        // get the name of this entry
                        if (string.Compare(key, "name", true, _culture) == 0)
                        {
                            user.Name = ParseString(values);
                        }
                        // get the given name of this entry
                        if (string.Compare(key, "givenname", true, _culture) == 0)
                        {
                            user.GivenName = ParseString(values);
                        }
                        // get the display name of this entry
                        if (string.Compare(key, "displayname", true, _culture) == 0)
                        {
                            user.DisplayName = ParseString(values);
                        }
                        // get the mail name of this entry
                        if (string.Compare(key, "mail", true, _culture) == 0)
                        {
                            user.Mail = ParseString(values);
                        }
                        // get the targetAddress name of this entry
                        if (string.Compare(key, "targetaddress", true, _culture) == 0)
                        {
                            user.TargetAddress = ParseString(values);
                        }
                        // get the sAMAccountName name of this entry
                        if (string.Compare(key, "samaccountname", true, _culture) == 0)
                        {
                            user.SamAccountName = ParseString(values);
                        }
                        // get the office address for this entry
                        if (string.Compare(key, "physicaldeliveryofficename", true, _culture) == 0)
                        {
                            user.PhysicalDeliveryOfficeName = ParseString(values);
                        }
                        // get the telephone number for this entry
                        if (string.Compare(key, "telephonenumber", true, _culture) == 0)
                        {
                            user.TelephoneNumber = ParseString(values);
                        }
                        // get the department for this entry
                        if (string.Compare(key, "department", true, _culture) == 0)
                        {
                            user.Department = ParseString(values);
                        }
                        // get the user principal name for this entry
                        if (string.Compare(key, "userprincipalname", true, _culture) == 0)
                        {
                            user.UserPrincipalName = ParseString(values);
                        }
                        // get permission groups for this entry
                        if (string.Compare(key, "memberof", true, _culture) == 0)
                        {
                            user.MemberOf = ParseString(values);
                        }
                        // get description for this entry
                        if (string.Compare(key, "description", true, _culture) == 0)
                        {
                            user.Description = ParseString(values);
                        }
                        // get company for this entry
                        if (string.Compare(key, "company", true, _culture) == 0)
                        {
                            user.Company = ParseString(values);
                        }
                        // get street address for this entry
                        if (string.Compare(key, "streetaddress", true, _culture) == 0)
                        {
                            user.StreetAddress = ParseString(values);
                        }
                        // get company for this entry
                        if (string.Compare(key, "postalcode", true, _culture) == 0)
                        {
                            user.PostalCode = ParseString(values);
                        }
                        // get company for this entry
                        if (string.Compare(key, "manager", true, _culture) == 0)
                        {
                            user.Manager = ParseString(values);
                        }
                        // get company for this entry
                        if (string.Compare(key, "st", true, _culture) == 0)
                        {
                            user.ST = ParseString(values);
                        }
                        // get mobile for this entry
                        if (string.Compare(key, "mobile", true, _culture) == 0)
                        {
                            user.Mobile = ParseString(values);
                        }
                        // get home phone for this entry
                        if (string.Compare(key, "homephone", true, _culture) == 0)
                        {
                            user.HomePhone = ParseString(values);
                        }
                        // get L for this entry
                        if (string.Compare(key, "l", true, _culture) == 0)
                        {
                            user.L = ParseString(values);
                        }
                        // get Location for this entry
                        if (string.Compare(key, "location", true, _culture) == 0)
                        {
                            user.Location = ParseString(values);
                        }
                        // get c for this entry
                        if (string.Compare(key, "c", true, _culture) == 0)
                        {
                            user.C = ParseString(values);
                        }
                        // get cn for this entry
                        if (string.Compare(key, "cn", true, _culture) == 0)
                        {
                            user.CN = ParseString(values);
                        }
                        // get when created for this entry
                        if (string.Compare(key, "whencreated", true, _culture) == 0)
                        {
                            user.WhenCreated = ParseString(values);
                        }
                        */
                    }
                }
                _userCollection.Add(user);
            }
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

