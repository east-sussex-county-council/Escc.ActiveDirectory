using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Text;

namespace Escc.ActiveDirectory
{
    /// <summary>
    /// Gets configuration settings from web.config or app.config
    /// </summary>
    public class ActiveDirectorySettingsFromConfiguration
    {
        private NameValueCollection _generalSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActiveDirectorySettingsFromConfiguration"/> class.
        /// </summary>
        public ActiveDirectorySettingsFromConfiguration()
        {
            _generalSettings = ConfigurationManager.GetSection("Escc.ActiveDirectory/GeneralSettings") as NameValueCollection;
        }

        /// <summary>
        /// Gets the default domain to be assumed when querying.
        /// </summary>
        /// <value>
        /// The default domain.
        /// </value>
        public string DefaultDomain
        {
            get
            {
                if (_generalSettings != null)
                {
                    return _generalSettings["DefaultDomain"];
                }
                return String.Empty;
            }
        }
    }
}
