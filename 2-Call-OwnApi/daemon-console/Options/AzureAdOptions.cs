using System;
using System.Globalization;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;

namespace daemon_console.Options
{
    /// <summary>
    /// Metadata designed to match application configurations for applications that call APIs.
    ///
    /// https://docs.microsoft.com/en-us/azure/active-directory/develop/scenario-web-app-sign-user-app-configuration?tabs=aspnetcore
    /// </summary>
    public class AzureAdOptions : ConfidentialClientApplicationOptions
    {
        public const string AzureAd = "AzureAd";

        /// <summary>
        /// The domain of the tenant
        /// </summary>
        public string Domain { get; set; }

        /// <summary>
        /// Callback path added to redirect URI
        /// </summary>
        public string CallbackPath { get; set; }

        /// <summary>
        /// URL of the authority
        /// </summary>
        public string Authority
        {
            get
            {
                return String.Format(CultureInfo.InvariantCulture, Instance + "{0}{1}", Domain, "/v2.0");
            }
        }

        /// <summary>
        /// Name of a certificate in the user certificate store
        /// </summary>
        /// <remarks>Daemon applications can authenticate with AAD through two mechanisms: ClientSecret
        /// (which is a kind of application password: the property above)
        /// or a certificate previously shared with AzureAD during the application registration 
        /// (and identified by this CertificateName property)
        /// <remarks> 
        public CertificateDescription Certificate { get; set; }

    }
}