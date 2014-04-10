using System;
using System.ComponentModel;

namespace EsccWebTeam.Data.ActiveDirectory
{
	/// <summary>
	/// Class corresponding to a subset of the AD user object properties.
	/// </summary>
	public class ADGroupMember
	{
		#region private fields
		/// <summary>
		/// private member field for the group name property value
		/// </summary>
		private string groupMember;
		/// <summary>
		/// private member field for the full member string property value
		/// </summary>
		private string memberString;
		/// <summary>
		/// private member field for the group name property value
		/// </summary>
		private string groupName;
		/// <summary>
		/// private member field for the samAccountName property value
		/// </summary>
		private string samAccountName;
		
		#endregion
		#region constructors, destructors and initialisers
		/// <summary>
		/// Class constructor. 
		/// </summary>
		public ADGroupMember()
		{
		}
		#endregion
		#region private methods
		#endregion
		#region public properties
		/// <summary>
		/// The name of the NT group.
		/// </summary>
		public string GroupName
		{
			get
			{
				return groupName;
			}
			set
			{
				groupName = value;
			}
		}
		/// <summary>
		/// The name of the group member.
		/// </summary>
		public string GroupMember
		{
			get
			{
				return groupMember;
			}
			set
			{
				groupMember = value;
			}
		}
		/// <summary>
		/// The fully qualified AD entry.
		/// </summary>
		public string MemberString
		{
			get
			{
				return memberString;
			}
			set
			{
				memberString = value;
			}
		}		
		/// <summary>
		/// The samAccountName AD entry.
		/// </summary>
		public string SamAccountName
		{
			get
			{
				return samAccountName;
			}
			set
			{
				samAccountName = value;
			}
		}		
		#endregion
	}
}
