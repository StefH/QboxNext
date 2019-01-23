namespace QboxNext.Qservice.Mvc
{
    /// <summary>
    /// CORS policy options that can be configured in appsettings.json
    /// </summary>
    public class CorsPolicyOptions
    {
        /// <summary>
        /// Gets or sets the allowed origins for the policy.
        /// </summary>
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        public string[] AllowedOrigins { get; set; } = new string[0];

        /// <summary>
        /// Gets or sets the HTTP methods.
        /// </summary>
        public string[] Methods { get; set; } = new string[0];
    }
}