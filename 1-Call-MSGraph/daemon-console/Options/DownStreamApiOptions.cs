namespace daemon_console.Options
{
    /// <summary>
    /// Metadata designed to match application configurations for applications that call APIs.
    ///
    /// https://docs.microsoft.com/en-us/azure/active-directory/develop/scenario-web-app-sign-user-app-configuration?tabs=aspnetcore
    /// </summary>
    public class DownstreamApiOptions
    {
        public const string DownstreamApi = "DownStreamApi";

        /// <summary>
        /// Base URL of the API being called
        /// </summary>
        public string BaseUrl { get; set; } = "https://graph.microsoft.com/v1.0/";
        /// <summary>
        /// Space seperated string of scopes to access from the API
        /// </summary>
        public string Scopes { get; set; } = "https://graph.microsoft.com/.default";
    }
}