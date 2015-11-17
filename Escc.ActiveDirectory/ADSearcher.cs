using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Configuration;
using System.DirectoryServices;
using System.Globalization;
using System.Reflection;
using System.Security.Principal;

namespace Escc.ActiveDirectory
{
    /// <summary>
    /// Summary description for ADSearcher.
    /// </summary>
    public class ADSearcher
    {
        #region private fields
        /// <summary>
        /// AD password stored in web.config
        /// </summary>
        private string ADPassword;
        /// <summary>
        /// AD user name stored in web.config
        /// </summary>
        private string ADUser;
        /// <summary>
        /// AD server path stored in web.config
        /// </summary>
        private string LDAPPath;
        /// <summary>
        /// private field to store user details
        /// </summary>
        private Collection<ActiveDirectoryUser> userCollection;
        /// <summary>
        /// private field to store group details
        /// </summary>
        private Collection<ActiveDirectoryGroup> groupsCollection;
        /// <summary>
        /// private field to store group names only
        /// </summary>
        private Collection<string> groupNames;
        /// <summary>
        /// flag used to influence search filter choice
        /// </summary>
        private bool searchBylogonFlag;
        /// <summary>
        /// flag used to influence search filter choice
        /// </summary>
        private bool searchByGroupNameFlag;
        /// <summary>
        /// member to hold current culture info
        /// </summary>
        private CultureInfo culture;
        private Collection<string> propertiesToLoad;
        private int maximumResults;

        #endregion
        #region constructors, destructors and initialisers
        /// <summary>
        /// Class constructor
        /// </summary>
        public ADSearcher()
        {
            ADUser = ConfigurationManager.AppSettings["ActiveDirectoryUser"];
            ADPassword = ConfigurationManager.AppSettings["ADPassword"];
            LDAPPath = ConfigurationManager.AppSettings["LDAPPath"];
            culture = CultureInfo.CurrentCulture;
            this.propertiesToLoad = new Collection<string>();
            this.maximumResults = 0;
        }
        #endregion

        #region Public properties

        /// <summary>
        /// Restricts the properties to load when searching for AD users. If collection is empty, all properties are loaded.
        /// </summary>
        public Collection<string> PropertiesToLoad
        {
            get { return propertiesToLoad; }
        }

        /// <summary>
        /// Gets or sets the maximum number of results to return (to speed up large queries)
        /// </summary>
        public int MaximumResults
        {
            get { return this.maximumResults; }
            set { this.maximumResults = value; }
        }

        #endregion

        #region public methods
        /// <summary>
        /// Checks to see if a user belongs to a domain group. Requires a list of group names.
        /// </summary>
        /// <param name="wi">System.Security.Principal.WindowsIdentity</param>
        /// <param name="groupNameCollection"></param>
        /// <returns>Returns a System.Collections.Specialized.NameValueCollection containing group names and a true/false string indicating membership.</returns>
        public static Dictionary<string,bool> GetGroupMembership(WindowsIdentity wi, IEnumerable<string> groupNameCollection)
        {
            var groupMembershipCollection = new Dictionary<string, bool>();
            bool isInRole;
            WindowsPrincipal wp = new WindowsPrincipal(wi);
            if (groupNameCollection != null)
            {
                foreach (string group in groupNameCollection)
                {
                    isInRole = wp.IsInRole(group);
                    groupMembershipCollection.Add(group, isInRole);
                }
            }

            return groupMembershipCollection;
        }
        
        /// <summary>
        /// Checks to see if a user belongs to a given NT group.
        /// </summary>
        /// <param name="wi">System.Security.Principal.WindowsIdentity</param>
        /// <param name="domainGroup">string</param>
        /// <returns>a boolean</returns>
        public static bool CheckGroupMembership(WindowsIdentity wi, string domainGroup)
        {
            WindowsPrincipal wp = new WindowsPrincipal(wi);
            return wp.IsInRole(domainGroup);
        }
        /// <summary>
        /// Gets an AD user object based on logon name.
        /// </summary>
        /// <param name="acctName">user logon name</param>
        /// <returns>An collection containing a single ActiveDirectoryUser object containing most properties associated with an AD user object.</returns>
        public Collection<ActiveDirectoryUser> GetUserBySamAccountName(string acctName)
        {
            searchBylogonFlag = true;
            SearchForUsers(acctName);
            searchBylogonFlag = false;

            foreach (ActiveDirectoryUser user in this.userCollection)
            {
                if (user.SamAccountName != null)
                {
                    string logonName = user.SamAccountName;
                    if (string.Compare(acctName, logonName, true, culture) == 0)
                    {
                        OnUserFound();
                        return this.userCollection;
                    }
                }
            }
            return this.userCollection;
        }

        /// <summary>
        /// Searches AD based on a partial username. If you have a full username use <seealso cref="GetUserBySamAccountName"/> instead.
        /// </summary>
        /// <param name="searchText">Part of an AD username</param>
        /// <returns>A collection of users with matching account names</returns>
        public Collection<ActiveDirectoryUser> SearchForUsersBySamAccountName(string searchText)
        {
            this.searchBylogonFlag = true;
            var users = this.SearchForUsers(searchText);
            this.searchBylogonFlag = false;
            return users;
        }
        /// <summary>
        /// Performs a search for AD users using ambiguous name resolution (i.e. searches can be done using partial names).
        /// </summary>
        /// <param name="searchText"></param>
        /// <returns>An collection containing multiple ActiveDirectoryUser objects.</returns>
        public Collection<ActiveDirectoryUser> SearchForUsers(string searchText)
        {
            this.userCollection = new Collection<ActiveDirectoryUser>();
            DirectoryEntry ent = new DirectoryEntry(LDAPPath);
            ent.Username = this.ADUser;
            ent.Password = this.ADPassword;
            DirectorySearcher ds = new DirectorySearcher();
            ds.SearchRoot = ent;
            if (searchBylogonFlag)
            {
                ds.Filter = "(&(saMAccountName=" + searchText + ")(objectClass=user)(objectCategory=Person))";
            }
            else
            {
                ds.Filter = "(&(anr=" + searchText + ")(objectClass=user)(objectCategory=Person))";
            }

            // If possible, restrict the properties to load to make the query faster
            if (this.propertiesToLoad.Count > 0)
            {
                string[] propertyNames = new string[this.propertiesToLoad.Count];
                this.propertiesToLoad.CopyTo(propertyNames, 0);
                ds.PropertiesToLoad.AddRange(propertyNames);
            }

            // Restrict the size too, again to make it faster
            if (this.maximumResults > 0) ds.SizeLimit = this.maximumResults;

            try
            {
                SearchResultCollection src = ds.FindAll();

                if (src.Count > 0)
                {
                    CreateUserCollection(src);
                }
                if (src.Count > 1)
                {
                    OnUsersFound();
                }
                if (src.Count == 1)
                {
                    OnUserFound();
                }
                if (src == null || src.Count == 0)
                {

                }
            }
            catch
            {
                throw;
            }
            finally
            {

                ds.Dispose();
                ent.Close();
            }
            return this.userCollection;
        }
        /// <summary>
        /// Gets an AD group object.
        /// </summary>
        /// <param name="groupName">string. The name of the group to retrieve</param>
        /// <returns>An collection containing a single ActiveDirectoryGroup containing ActiveDirectoryGroupMember objects. </returns>
        public Collection<ActiveDirectoryGroup> GetGroupByGroupName(string groupName)
        {
            searchByGroupNameFlag = true;
            SearchForGroups(groupName);
            foreach (ActiveDirectoryGroup group in this.groupsCollection)
            {
                foreach (ActiveDirectoryGroupMember member in group)
                {
                    if (member.GroupName == groupName)
                    {
                        OnGroupFound();
                        return this.groupsCollection;
                    }
                }
            }
            return this.groupsCollection;
        }
        /// <summary>
        /// Finds groups based on ambiguous name resolution
        /// </summary>
        /// <param name="searchText">string</param>
        /// <returns>An collection containing ActiveDirectoryGroup objects</returns>
        public Collection<ActiveDirectoryGroup> SearchForGroups(string searchText)
        {
            this.groupsCollection = new Collection<ActiveDirectoryGroup>();
            DirectoryEntry ent = new DirectoryEntry(LDAPPath);
            DirectorySearcher ds = new DirectorySearcher();
            ds.SearchRoot = ent;
            if (searchByGroupNameFlag)
            {
                ds.Filter = "(&(CN=" + searchText + ")(objectClass=group))";
            }
            else
            {
                ds.Filter = "(&(anr=" + searchText + ")(objectClass=group))";
            }
            try
            {
                SearchResultCollection src = ds.FindAll();

                if (src.Count > 0)
                {
                    //CreateGroupColl(src);
                    CreateGroupCollection(src);
                }
                if (src.Count > 1)
                {
                    OnGroupsFound();
                }
                if (src.Count == 1)
                {
                    OnGroupFound();
                }
                if (src == null || src.Count == 0)
                {

                }
            }
            catch
            {
                throw;
            }
            finally
            {
                ds.Dispose();
                ent.Close();
            }
            return this.groupsCollection;
        }
        /// <summary>
        /// Gets a collection of ADS groups based on ambiguous name resolution
        /// </summary>
        /// <param name="searchText"></param>
        /// <returns>A System.DirectoryServices.SearchResultCollection</returns>
        public SearchResultCollection GetGroups(string searchText)
        {
            DirectoryEntry ent = new DirectoryEntry(LDAPPath);
            DirectorySearcher ds = new DirectorySearcher();
            ds.SearchRoot = ent;
            ds.Filter = "(&(anr=" + searchText + ")(objectClass=group))";
            SearchResultCollection src = ds.FindAll();
            ds.Dispose();
            ent.Close();
            return src;
        }
        /// <summary>
        /// Finds group names based on ambiguous name resolution
        /// </summary>
        /// <param name="searchText">Group name with or without wildcard (e.g. "SOMEGROUP_*")</param>
        /// <returns>Group names as strings</returns>
        public Collection<string> GetGroupNames(string searchText)
        {
            groupNames = new Collection<string>();
            DirectoryEntry ent = new DirectoryEntry(LDAPPath);
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
                groupNames.Add(path);
            }
            return this.groupNames;
        }
        /// <summary>
        /// Finds group paths based on ambiguous name resolution
        /// </summary>
        /// <param name="searchText">Group name with or without wildcard (e.g. "SOMEGROUP_*")</param>
        /// <returns>Group paths as strings</returns>
        public Collection<string> GetGroupPaths(string searchText)
        {
            groupNames = new Collection<string>();
            DirectoryEntry ent = new DirectoryEntry(LDAPPath);
            DirectorySearcher ds = new DirectorySearcher();
            ds.SearchRoot = ent;
            ds.Filter = "(&(anr=" + searchText + ")(objectClass=group))";
            SearchResultCollection src = ds.FindAll();
            foreach (SearchResult res in src)
            {
                groupNames.Add(res.Path);
            }
            return this.groupNames;
        }
        #endregion
        #region private methods
        /// <summary>
        /// Helper method populates a collection of groups.
        /// </summary>
        /// <param name="Results">System.DirectoryServices.SearchResultCollection</param>
        private void CreateGroupCollection(SearchResultCollection Results)
        {
            string groupName = null;
            string samAccountName = null;
            foreach (SearchResult groupObject in Results)
            {
                ActiveDirectoryGroup group = new ActiveDirectoryGroup();
                PropertyCollection propcoll = groupObject.GetDirectoryEntry().Properties;
                foreach (string key in groupObject.GetDirectoryEntry().Properties.PropertyNames)
                {
                    //loop through all the values associated with our key
                    foreach (object values in propcoll[key])
                    {
                        // get the group name of this entry
                        if (string.Compare(key, "cn", true, culture) == 0)
                        {
                            groupName = values.ToString();
                        }
                        // get the samAccountName name of this entry
                        if (string.Compare(key, "samAccountName", true, culture) == 0)
                        {
                            samAccountName = values.ToString();
                        }
                        // get the member name of this entry
                        if (string.Compare(key, "member", true, culture) == 0)
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
                    groupsCollection.Add(group);
                }
            }
        }

        /// <summary>
        /// Helper method populates collection with ActiveDirectoryUser objects.
        /// </summary>
        /// <param name="Results">System.DirectoryServices.SearchResultCollection</param>
        private void CreateUserCollection(SearchResultCollection Results)
        {
            string[] propertyNames = null;
            if (this.PropertiesToLoad.Count > 0)
            {
                propertyNames = new string[this.PropertiesToLoad.Count];
                this.PropertiesToLoad.CopyTo(propertyNames, 0);
            }

            foreach (SearchResult r in Results)
            {
                PropertyCollection propcoll = r.GetDirectoryEntry().Properties;
                ActiveDirectoryUser user = new ActiveDirectoryUser();

                if (this.PropertiesToLoad.Count == 0)
                {
                    propertyNames = new string[propcoll.Count];
                    propcoll.PropertyNames.CopyTo(propertyNames, 0);
                }

                //loop through all of the properties for this record
                foreach (string key in propertyNames)
                {
                    // New faster bit of code to read property values. This could potentially replace the old code below
                    // if (1) every property always has a PropertyValueCollection (2) the lowercase AD property names
                    // can be made to relate to the mixed case object property names. Not all scenarios tested so old
                    // code left as default behaviour, and this code kicks in instead for new/updated apps that specify 
                    // the PropertiesToLoad.
                    if (this.PropertiesToLoad.Count > 0)
                    {
                        PropertyValueCollection propertyValues = propcoll[key] as PropertyValueCollection;
                        if (propertyValues != null && propertyValues.Count > 0)
                        {
                            // AD potentially holds multiple values for a property, but the ActiveDirectoryUser object only 
                            // supports one for each, so just use the first value. The old code below just used
                            // the last value because it overwrites any previous one found.
                            PropertyInfo userProperty = user.GetType().GetProperty(key) as PropertyInfo;
                            if (userProperty != null) userProperty.SetValue(user, ParseString(propertyValues[0]), null);
                        }

                        // Skip back to the start of the loop - don't run the old code.
                        continue;
                    }


                    // Old approach still used for generic queries that don't specify PropertiesToLoad (effectively SELECT * queries). 
                    // Loop through all the values associated with our key, testing every possible key they might match.
                    foreach (object values in propcoll[key])
                    {
                        // get the title of this entry 
                        if (string.Compare(key, "title", true, culture) == 0)
                        {
                            user.Title = ParseString(values);
                        }
                        // get the title of this entry  
                        if (string.Compare(key, "sn", true, culture) == 0)
                        {
                            user.SN = ParseString(values);
                        }
                        // get the distinguished name of this entry
                        if (string.Compare(key, "distinguishedname", true, culture) == 0)
                        {
                            user.DistinguishedName = ParseString(values);
                        }
                        // get the name of this entry
                        if (string.Compare(key, "name", true, culture) == 0)
                        {
                            user.Name = ParseString(values);
                        }
                        // get the given name of this entry
                        if (string.Compare(key, "givenname", true, culture) == 0)
                        {
                            user.GivenName = ParseString(values);
                        }
                        // get the display name of this entry
                        if (string.Compare(key, "displayname", true, culture) == 0)
                        {
                            user.DisplayName = ParseString(values);
                        }
                        // get the mail name of this entry
                        if (string.Compare(key, "mail", true, culture) == 0)
                        {
                            user.Mail = ParseString(values);
                        }
                        // get the targetAddress name of this entry
                        if (string.Compare(key, "targetaddress", true, culture) == 0)
                        {
                            user.TargetAddress = ParseString(values);
                        }
                        // get the sAMAccountName name of this entry
                        if (string.Compare(key, "samaccountname", true, culture) == 0)
                        {
                            user.SamAccountName = ParseString(values);
                        }
                        // get the office address for this entry
                        if (string.Compare(key, "physicaldeliveryofficename", true, culture) == 0)
                        {
                            user.PhysicalDeliveryOfficeName = ParseString(values);
                        }
                        // get the telephone number for this entry
                        if (string.Compare(key, "telephonenumber", true, culture) == 0)
                        {
                            user.TelephoneNumber = ParseString(values);
                        }
                        // get the department for this entry
                        if (string.Compare(key, "department", true, culture) == 0)
                        {
                            user.Department = ParseString(values);
                        }
                        // get the user principal name for this entry
                        if (string.Compare(key, "userprincipalname", true, culture) == 0)
                        {
                            user.UserPrincipalName = ParseString(values);
                        }
                        // get permission groups for this entry
                        if (string.Compare(key, "memberof", true, culture) == 0)
                        {
                            user.MemberOf = ParseString(values);
                        }
                        // get description for this entry
                        if (string.Compare(key, "description", true, culture) == 0)
                        {
                            user.Description = ParseString(values);
                        }
                        // get company for this entry
                        if (string.Compare(key, "company", true, culture) == 0)
                        {
                            user.Company = ParseString(values);
                        }
                        // get street address for this entry
                        if (string.Compare(key, "streetaddress", true, culture) == 0)
                        {
                            user.StreetAddress = ParseString(values);
                        }
                        // get company for this entry
                        if (string.Compare(key, "postalcode", true, culture) == 0)
                        {
                            user.PostalCode = ParseString(values);
                        }
                        // get company for this entry
                        if (string.Compare(key, "manager", true, culture) == 0)
                        {
                            user.Manager = ParseString(values);
                        }
                        // get company for this entry
                        if (string.Compare(key, "st", true, culture) == 0)
                        {
                            user.ST = ParseString(values);
                        }
                        // get mobile for this entry
                        if (string.Compare(key, "mobile", true, culture) == 0)
                        {
                            user.Mobile = ParseString(values);
                        }
                        // get home phone for this entry
                        if (string.Compare(key, "homephone", true, culture) == 0)
                        {
                            user.HomePhone = ParseString(values);
                        }
                        // get L for this entry
                        if (string.Compare(key, "l", true, culture) == 0)
                        {
                            user.L = ParseString(values);
                        }
                        // get Location for this entry
                        if (string.Compare(key, "location", true, culture) == 0)
                        {
                            user.Location = ParseString(values);
                        }
                        // get c for this entry
                        if (string.Compare(key, "c", true, culture) == 0)
                        {
                            user.C = ParseString(values);
                        }
                        // get cn for this entry
                        if (string.Compare(key, "cn", true, culture) == 0)
                        {
                            user.CN = ParseString(values);
                        }
                    }
                }

                userCollection.Add(user);
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
        /// Delegate for the GroupFound event
        /// </summary>
        public delegate void GroupFoundEventHandler(object sender, GroupFoundEventArgs e);
        /// <summary>
        /// 
        /// </summary>
        protected void OnGroupFound()
        {
            // check there are handlers for the event before raising
            if (this.GroupFound != null)
            {
                // raise event
                this.GroupFound(this, new GroupFoundEventArgs(this.groupsCollection));
            }
        }
        /// <summary>
        /// Event indicating that multiple groups have been found by search term
        /// </summary>
        public event GroupsFoundEventHandler GroupsFound;
        /// <summary>
        /// Delegate for the GroupsFound event
        /// </summary>
        public delegate void GroupsFoundEventHandler(object sender, GroupsFoundEventArgs e);
        /// <summary>
        /// 
        /// </summary>
        protected void OnGroupsFound()
        {
            // check there are handlers for the event before raising
            if (this.GroupsFound != null)
            {
                // raise event
                this.GroupsFound(this, new GroupsFoundEventArgs(this.groupsCollection));
            }
        }
        /// <summary>
        /// Event indicating that a user has been found by user principal name
        /// </summary>
        public event UserFoundEventHandler UserFound;
        /// <summary>
        /// Delegate for the UserFound event
        /// </summary>
        public delegate void UserFoundEventHandler(object sender, UserFoundEventArgs e);
        /// <summary>
        /// 
        /// </summary>
        protected void OnUserFound()
        {
            // check there are handlers for the event before raising
            if (this.UserFound != null)
            {
                // raise event
                this.UserFound(this, new UserFoundEventArgs(this.userCollection));
            }
        }
        /// <summary>
        /// Event indicating that a user or users have has been found by search term
        /// </summary>
        public event UsersFoundEventHandler UsersFound;
        /// <summary>
        /// Delegate for the UsersFound event
        /// </summary>
        public delegate void UsersFoundEventHandler(object sender, UsersFoundEventArgs e);
        /// <summary>
        /// 
        /// </summary>
        protected void OnUsersFound()
        {
            // check there are handlers for the event before raising
            if (this.UsersFound != null)
            {
                // raise event
                this.UsersFound(this, new UsersFoundEventArgs(this.userCollection));
            }
        }
        #endregion
    }
    #region custom eventargs classes
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
    #endregion
}

