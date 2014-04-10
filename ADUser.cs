using System;
using System.ComponentModel;
using System.Globalization;
using System.Collections;

namespace EsccWebTeam.Data.ActiveDirectory
{
	/// <summary>
	/// Class corresponding to an Active Direstory User object or subset of the user object properties
	/// </summary>
	public class ADUser
	{
		#region private fields
		/// <summary>
		/// private member field for the AD 'Title' property value
		/// </summary>
		private string title;
		/// <summary>
		/// private member field for the AD 'SN' property value
		/// </summary>
		private string sn;
		/// <summary>
		/// private member field for the AD 'Name' property value
		/// </summary>
		private string name;
		/// <summary>
		/// private member field for the AD 'GivenName' property value
		/// </summary>
		private string givenName;
		/// <summary>
		/// private member field for the AD 'DisplayName' property value
		/// </summary>
		private string displayName;
		/// <summary>
		/// private member field for the AD 'DistinguishedName' property value
		/// </summary>
		private string distinguishedName;
		/// <summary>
		/// private member field for the AD 'sAMAccountName' property value
		/// </summary>
		private string samAccountName;
		/// <summary>
		/// private member field for the AD 'PhysicalDeliveryOfficeName' property value
		/// </summary>
		private string physicalDeliveryOfficeName;
		/// <summary>
		/// private member field for the AD 'MemberOf' property values
		/// </summary>
		private string memberof;
		/// <summary>
		/// private member field for the AD 'UserPrincipalName' property value
		/// </summary>
		private string userPrincipalName;
		/// <summary>
		/// private member field for the AD 'TelephoneNumber' property value
		/// </summary>
		private string telephoneNumber;
		/// <summary>
		/// private member field for the AD 'Mobile' property value
		/// </summary>
		private string mobile;
		/// <summary>
		/// private member field for the AD 'HomePhone' property value
		/// </summary>
		private string homePhone;
		/// <summary>
		/// private member field for the AD 'TargetAddress' property value
		/// </summary>
		private string targetAddress;
		/// <summary>
		/// private member field for the AD 'Mail' property value
		/// </summary>
		private string mail;
		/// <summary>
		/// private member field for the AD 'Department' property value
		/// </summary>
		private string department;
		/// <summary>
		/// private member field for the AD 'Description' property value
		/// </summary>
		private string description;
		/// <summary>
		/// private member field for the AD 'Company' property value
		/// </summary>
		private string company;
		/// <summary>
		/// private member field for the AD 'Manager' property value
		/// </summary>
		private string manager;
		/// <summary>
		/// private member field for the AD 'StreetAddress' property value
		/// </summary>
		private string streetAddress;
		/// <summary>
		/// private member field for the AD 'PostalCode' property value
		/// </summary>
		private string postalCode;
		/// <summary>
		/// private member field for the AD 'ST' property value
		/// </summary>
		private string st;
		/// <summary>
		/// private member field for the AD 'L' property value
		/// </summary>
		private string l;
		/// <summary>
		/// private member field for the AD 'Location' property value
		/// </summary>
		private string location;
		/// <summary>
		/// private member field for the AD 'C' property value
		/// </summary>
		private string c;
		/// <summary>
		/// private member field for the AD 'CN' property value
		/// </summary>
		private string cn;

        /* BROKEN
          /// <summary>
          /// private member field for the AD 'ObjectGuid' property value
          /// </summary>
          private string objectguid;

          /// <summary>
          /// private member field for the AD 'WhenChanged' property value
          /// </summary>
          private string whenchanged;

      */


        /// <summary>
		/// private memeber field to store memberof groups as a string array
		/// </summary>
		private ArrayList arrMemberGroups;
		#endregion
		#region constructors, destructors and initialisers
		/// <summary>
		/// 
		/// </summary>
		public ADUser()
		{
			arrMemberGroups = new ArrayList();
		}
		#endregion
		#region private methods
		/// <summary>
		/// 
		/// </summary>
		/// <param name="val"></param>
		private void BuildGroupString(string val)
		{
			memberof += val + ",";
		}
		#endregion
		#region public properties
		/// <summary>
		/// 
		/// </summary>
		public string Title
		{
			get
			{
				return title;
			}
			set
			{
				title = value;
			}
		}
		/// <summary>
		/// 
		/// </summary>
		public string SN
		{
			get
			{
				return sn;
			}
			set
			{
				sn = value;
			}
		}
		/// <summary>
		/// 
		/// </summary>
		public string DisplayName
		{
			get
			{
				return displayName;
			}
			set
			{
				displayName = value;
			}
		}
		/// <summary>
		/// 
		/// </summary>
		public string TargetAddress
		{
			get
			{
				return targetAddress;
			}
			set
			{
				targetAddress = value;
			}
		}
		/// <summary>
		/// 
		/// </summary>
		public string DistinguishedName
		{
			get
			{
				return distinguishedName;
			}
			set
			{
				distinguishedName = value;
			}
		}
		/// <summary>
		/// 
		/// </summary>
		public string Name
		{
			get
			{
				return name;
			}
			set
			{
				name = value;
			}
		}
		/// <summary>
		/// 
		/// </summary>
		public string Company
		{
			get
			{
				return company;
			}
			set
			{
				company = value;
			}
		}
		/// <summary>
		/// 
		/// </summary>
		public string Department
		{
			get
			{
				return department;
			}
			set
			{
				department = value;
			}
		}
		/// <summary>
		/// 
		/// </summary>
		public string GivenName
		{
			get
			{
				return givenName;
			}
			set
			{
				givenName = value;
			}
		}
		/// <summary>
		/// 
		/// </summary>
		public string PhysicalDeliveryOfficeName
		{
			get
			{
				return physicalDeliveryOfficeName;
			}
			set
			{
				physicalDeliveryOfficeName = value;
			}
		}
		/// <summary>
		/// 
		/// </summary>
		public string MemberOf
		{
			get
			{
				return memberof;
			}
			set
			{
				BuildGroupString(value);
			}
		}
		/// <summary>
		/// 
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
		/// <summary>
		/// 
		/// </summary>
		public string UserPrincipalName
		{
			get
			{
				return userPrincipalName;
			}
			set
			{
				userPrincipalName = value;
			}
		}
		/// <summary>
		/// 
		/// </summary>
		public string Mail
		{
			get
			{
				return mail;
			}
			set
			{
				mail = value;
			}
		}
		/// <summary>
		/// 
		/// </summary>
		public string TelephoneNumber
		{
			get
			{
				return telephoneNumber;
			}
			set
			{
				telephoneNumber = value;
			}
		}
		/// <summary>
		/// 
		/// </summary>
		public string Description
		{
			get
			{
				return description;
			}
			set
			{
				description = value;
			}
		}
		/// <summary>
		/// 
		/// </summary>
		public string Manager
		{
			get
			{
				return manager;
			}
			set
			{
				manager = value;
			}
		}
		/// <summary>
		/// 
		/// </summary>
		public string StreetAddress
		{
			get
			{
				return streetAddress;
			}
			set
			{
				streetAddress = value;
			}
		}
		/// <summary>
		/// 
		/// </summary>
		public string PostalCode
		{
			get
			{
				return postalCode;
			}
			set
			{
				postalCode = value;
			}
		}
		/// <summary>
		/// 
		/// </summary>
		public string ST
		{
			get
			{
				return st;
			}
			set
			{
				st = value;
			}
		}
		/// <summary>
		/// 
		/// </summary>
		public string Mobile
		{
			get
			{
				return mobile;
			}
			set
			{
				mobile = value;
			}
		}
		/// <summary>
		/// 
		/// </summary>
		public string L
		{
			get
			{
				return l;
			}
			set
			{
				l = value;
			}
		}
		/// <summary>
		/// 
		/// </summary>
		public string Location
		{
			get
			{
				return location;
			}
			set
			{
				location = value;
			}
		}
		/// <summary>
		/// 
		/// </summary>
		public string HomePhone
		{
			get
			{
				return homePhone;
			}
			set
			{
				homePhone = value;
			}
		}
		/// <summary>
		/// 
		/// </summary>
		public string C
		{
			get
			{
				return c;
			}
			set
			{
				c = value;
			}
		}
		/// <summary>
		/// 
		/// </summary>
		public string CN
		{
			get
			{
				return cn;
			}
			set
			{
				cn = value;
			}
		}

        /* BROKEN
        /// <summary>
        /// 
        /// </summary>
        public string ObjectGuid
        {
            get
            {

                return objectguid;
            }
            set
            {
                objectguid = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string WhenCreated
        {
            get
            {
                return whenchanged;
            }
            set
            {
                whenchanged = value;
            }
        }
        */
      
		/// <summary>
		/// 
		/// </summary>
		public ArrayList MemberGroups
		{
			get
			{
				arrMemberGroups.Clear();
				Char[] separator = new Char[]{Convert.ToChar(",", CultureInfo.InvariantCulture)};
				
				string[] temp = this.memberof.Split(separator);
				foreach(string s in temp)
				{
					arrMemberGroups.Add(s);
				}
				return arrMemberGroups;
			}			 
		}
		#endregion
	}
}
