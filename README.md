# Escc.ActiveDirectory

A library for querying users and groups in Active Directory.

## Check user permissions in ASP.NET

Check whether the current user of an ASP.NET application is a member of an Active Directory group using the information already available in ASP.NET, storing the result in session to avoid repeated queries by the application.

	var defaultDomain = new ActiveDirectorySettingsFromConfiguration().DefaultDomain;
	var sessionCache = new SessionPermissionsResultCache();
	var permissions = new UserGroupMembership(new LogonIdentityUserGroupsProvider(), defaultDomain, sessionCache);
	var result = permissions.UserIsInGroup(new [] { "group1, "group2" });

## Impersonate an account

When an ASPX page tries to use protected resources (such as files or folders) on a server that is different from the IIS server receiving the original web request, the credentials of the user who is making the original request are not passed to the second server. 

	// Impersonate an account that has sufficent permissions on the resource you wish to access	
	var username = "example";
	var domain = "example";
	var password = "example";

	var impersonator = new ImpersonationWrapper();
	impersonator.ImpersonateUser(username, domain, password);

	// Access the protected resource
	...

	// End the impersonation, returning the identity to its original value
	impersonator.UndoUserImpersonation();

The interface `IImpersonationWrapper` lets you specify your own implementations of `ImpersonationWrapper`; 

## Configuration settings

Some settings can be saved in `web.config` or `app.config`.

	<configuration>
	  <configSections>
	    <sectionGroup name="Escc.ActiveDirectory">
	      <section name="GeneralSettings" type="System.Configuration.NameValueSectionHandler, System, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" />
	    </sectionGroup>
	  </configSections>
	  <Escc.ActiveDirectory>
	    <GeneralSettings>
	      <add key="DefaultDomain" value="example" />
	    </GeneralSettings>
	  </Escc.ActiveDirectory>
	</configuration>

