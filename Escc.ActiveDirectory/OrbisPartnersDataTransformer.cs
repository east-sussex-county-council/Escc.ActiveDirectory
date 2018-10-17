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

            FixCompanyField(user);
            FixDepartmentField(user);
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
                        if (user.Department.StartsWith("BUS ")) user.Department = "Business Services";
                        if (user.Department.StartsWith("CSF ")) user.Department = "Children's Services";
                        if (user.Department.StartsWith("FIN ")) user.Department = "Finance Service";
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
