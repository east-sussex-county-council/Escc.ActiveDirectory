using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escc.ActiveDirectory
{
    /// <summary>
    /// Sometimes Active Directory data is out of date or in unhelpful formats. 
    /// For known patterns in Orbis partners' directories, transform the information to be something more useful.
    /// </summary>
    public class OrbisPartnersDataTransformer
    {
        private static readonly Dictionary<string, string> _surreyDepartments = new Dictionary<string, string>()
        {
            { "BUS", "Business Services" },
            { "CSF", "Children, Families, Learning and Communities" },
            { "FIN", "Finance Service" },
            { "ASC", "Health, Wellbeing and Adult Social Care" },
            { "LDC", String.Empty },
            { "EI", String.Empty }
        };

        /// <summary>
        /// Transforms the data for multiple users.
        /// </summary>
        /// <param name="users">The users.</param>
        /// <exception cref="ArgumentNullException">users</exception>
        public void TransformUsers(IEnumerable<ActiveDirectoryUser> users)
        {
            if (users == null)
            {
                throw new ArgumentNullException(nameof(users));
            }

            foreach (var user in users) TransformUser(user);
        }

        /// <summary>
        /// Transforms the data for one user.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <exception cref="ArgumentNullException">user</exception>
        public void TransformUser(ActiveDirectoryUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            user.Name = FixName(user.Name);
            user.DisplayName = FixName(user.DisplayName);
            FixPhoneField(user);
            FixCompanyField(user);
            FixDepartmentField(user);
        }

        /// <summary>
        /// Surrey puts the department abbreviation after the name
        /// </summary>
        /// <param name="name">The user's name.</param>
        private string FixName(string name)
        {
            if (!String.IsNullOrEmpty(name))
            {
                foreach (var departmentAbbreviation in _surreyDepartments.Keys)
                {
                    if (name.EndsWith(" " + departmentAbbreviation))
                    {
                        name = name.Substring(0, name.Length - departmentAbbreviation.Length - 1);
                    }
                }
            }

            return name;
        }

        /// <summary>
        /// Surrey put free text notes in their phone field, but there are some common phrases we can look for to remove them.
        /// </summary>
        /// <param name="user">The user.</param>
        private void FixPhoneField(ActiveDirectoryUser user)
        {
            if (String.IsNullOrEmpty(user.TelephoneNumber)) return;

            if (user.TelephoneNumber.Contains("public use number") || user.TelephoneNumber.Contains("supply your phone number"))
            {
                user.TelephoneNumber = String.Empty;
            }

            // Normalise punctuation
            user.TelephoneNumber = user.TelephoneNumber.Replace("(", String.Empty).Replace(")", String.Empty);
      }

        /// <summary>
        /// Surrey don't populate their Company field, and East Sussex uses an abbreviation.
        /// </summary>
        /// <param name="user">The user.</param>
        private static void FixCompanyField(ActiveDirectoryUser user)
        {
            if (user.Mail != null && user.Mail.ToUpperInvariant().Contains("SURREYCC.GOV.UK"))
            {
                user.Company = "Surrey County Council";
            }

            if (user.Company == "ESCC") user.Company = "East Sussex County Council";
        }

        /// <summary>
        /// East Sussex departments are out of date and Surrey's are abbreviated.
        /// </summary>
        /// <param name="user">The user.</param>
        private static void FixDepartmentField(ActiveDirectoryUser user)
        {
            switch (user.Company)
            {
                // Surrey puts a department abbreviation and a team in the Department field
                case "Surrey County Council":
                    if (!string.IsNullOrEmpty(user.Department))
                    {
                        foreach (var departmentAbbreviation in _surreyDepartments.Keys)
                        {
                            if (user.Department.StartsWith(departmentAbbreviation + " ") && !String.IsNullOrEmpty(_surreyDepartments[departmentAbbreviation]))
                            {
                                user.Department = _surreyDepartments[departmentAbbreviation];
                            }
                        }
                    }
                    break;

                // East Sussex has outdated department names in its Department field, and uses an unnecessary 'Department' suffix
                case "East Sussex County Council":
                    if (!string.IsNullOrEmpty(user.Department))
                    {
                        switch (user.Department)
                        {
                            case "Adult Social Care Department":
                                user.Department = "Adult Social Care and Health";
                                break;
                            case "Children's Services Department":
                                user.Department = "Children's Services";
                                break;
                            case "Corporate Resources Directorate":
                            case "Business Services Department":
                                user.Department = "Business Services";
                                break;
                            case "Transport and Environment Department":
                            case "Economy, Transport and Environment Department":
                                user.Department = "Communities, Economy and Transport";
                                break;
                            case "Governance and Community Services":
                                user.Department = "Governance Services";
                                break;
                        }
                    }
                    break;
            }

        }
    }
}
