# Escc.ActiveDirectory

A library for querying users and groups in Active Directory.

## Check user permissions in ASP.NET

Check whether the current user of an ASP.NET application is a member of an Active Directory group using the information already available in ASP.NET, storing the result in session to avoid repeated queries by the application.

	var defaultDomain = new ActiveDirectorySettingsFromConfiguration().DefaultDomain;
	var sessionCache = new SessionPermissionsResultCache();
	var permissions = new UserGroupMembership(new LogonIdentityUserGroupsProvider(), defaultDomain, sessionCache);
	var result = permissions.UserIsInGroup(new [] { "group1, "group2" });