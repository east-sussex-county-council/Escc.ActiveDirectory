# Escc.ActiveDirectory

A library for querying users and groups in Active Directory.

## Check whether a user is in a group

You can check whether the current user of an ASP.NET application is a member of an Active Directory group (or list of groups) using the information already available in ASP.NET, storing the result in session to avoid repeated queries by the application. 

	var defaultDomain = new ActiveDirectorySettingsFromConfiguration().DefaultDomain;
	var sessionCache = new SessionPermissionsResultCache();
	var permissions = new LogonIdentityGroupMembershipChecker(defaultDomain, sessionCache);
	bool result = permissions.UserIsInGroup(new [] { "group1, "group2" });
	Dictionary<string, bool> groupResults = permissions.UserIsInGroups(new [] { "group1, "group2" });

You can also check whether a user is in a group (or list of groups) based on their `WindowsIdentity`. When using a `WindowsIdentity` the `defaultDomain` and `resultCache` options are not supported.

	var userToCheck = WindowsIdentity.GetCurrent();
	var permissions = new WindowsIdentityGroupMembershipChecker(userToCheck);
	bool result = permissions.UserIsInGroup(new [] { "group1, "group2" })
	Dictionary<string, bool> groupResults = permissions.UserIsInGroups(new[] { "group1", "group2" })

Both of these classes implement the `IGroupMembershipChecker` interface.

## Look up a user or users

This requires an `LdapActiveDirectorySearcher` created with instance with LDAP connection settings (see below). Some methods use [ambiguous name resolution](http://social.technet.microsoft.com/wiki/contents/articles/22653.active-directory-ambiguous-name-resolution.aspx).

	var settings = new ActiveDirectorySettingsFromConfiguration();	
	var searcher = new LdapActiveDirectorySearcher(settings.LdapPath, settings.LdapUsername, settings.LdapPassword);
	var propertiesToLoad = new [] { "displayname", "mail" };
	
	// Get one user when you know the username
	searcher.GetUserBySamAccountName("exampleuser", propertiesToLoad); 
 
	// Get multiple users where you know part of the username
 	searcher.SearchForUsersBySamAccountName("incompleteuserna", propertiesToLoad);

	// Get multiple users using ambiguous name resolution
    searcher.SearchForUsers("example", IList<string> propertiesToLoad);

The result is returned much faster if you specify just the properties you need, but if the `propertiesToLoad` argument is `null` or has no items then all available properties will be returned. The possible properties are:

- title
- sn
- distinguishedname
- name
- givenname
- displayname
- mail
- targetaddress
- samaccountname
- physicaldeliveryofficename
- telephonenumber
- department
- userprincipalname
- memberof
- description
- company
- streetaddress
- postalcode
- manager
- st
- mobile
- homephone
- l
- location
- c
- cn

## Look up a group or groups

This requires an `LdapActiveDirectorySearcher` created with instance with LDAP connection settings (see below). 

When using [ambiguous name resolution](http://social.technet.microsoft.com/wiki/contents/articles/22653.active-directory-ambiguous-name-resolution.aspx) you can optionally search using a wildcard (eg "groupname*").

	var settings = new ActiveDirectorySettingsFromConfiguration();
	var searcher = new LdapActiveDirectorySearcher(settings.LdapPath, settings.LdapUsername, settings.LdapPassword);
    
	// Get one group when you know the name
	searcher.GetGroupByGroupName("groupname");

	// Get group objects based on ambiguous name resolution    
 	searcher.SearchForGroups("incompletegroupna");

	// Get group names based on ambiguous name resolution    
    searcher.GetGroupNames("incompletegroupna");
    
	// Get group paths based on ambiguous name resolution    
    searcher.GetGroupPaths("incompletegroupna");

The interface `IActiveDirectorySearcher` lets you specify your own implementations of `LdapActiveDirectorySearcher`.

## Impersonate an account

When an ASPX page tries to use protected resources (such as files or folders) on a server that is different from the IIS server receiving the original web request, the credentials of the user who is making the original request are not passed to the second server. 

	// Impersonate an account that has sufficent permissions on the resource you wish to access	
	var username = "example";
	var domain = "example";
	var password = "example";

	var impersonator = new ImpersonatorWrapper();
	impersonator.ImpersonateUser(username, domain, password);

	// Access the protected resource
	...

	// End the impersonation, returning the identity to its original value
	impersonator.UndoUserImpersonation();

The interface `IImpersonationWrapper` lets you specify your own implementations of `ImpersonatorWrapper`; 

## Configuration settings

Some settings can be saved in `web.config` or `app.config` and read back using `new ActiveDirectorySettingsFromConfiguration()`.

	<configuration>
	  <configSections>
	    <sectionGroup name="Escc.ActiveDirectory">
	      <section name="GeneralSettings" type="System.Configuration.NameValueSectionHandler, System, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" />
	    </sectionGroup>
	  </configSections>
	  <Escc.ActiveDirectory>
	    <GeneralSettings>

		  <!-- The default domain to assume when dealing with users on a single domain -->
	      <add key="DefaultDomain" value="example" />

		  <!-- Connection details used when querying using LDAP -->
		  <add key="LdapPath" value="example" />
	      <add key="LdapUser" value="example" />
		  <add key="LdapPassword" value="example" />

	    </GeneralSettings>
	  </Escc.ActiveDirectory>
	</configuration>

## NuGet package

The NuGet package is created using the [NuBuild](https://github.com/bspell1/NuBuild) extension for Visual Studio.