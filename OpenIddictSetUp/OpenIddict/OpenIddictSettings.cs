namespace OpenIddictSetUp.OpenIddict
{
    public class OpenIddictSettings
    {
        public OpenIddictSettings(OpenIddictServerBuilder openIddictServerBuilder)
        {
            OpenIddictServerBuilder = openIddictServerBuilder;
        }
        /// <summary>
        /// PublicUrl will be prepended to relative URIs in 'RedirectUris' and 'PostLogoutRedirectUris'
        /// </summary>
        public string PublicUrl { get; set; } = string.Empty;
        /// <summary>
        /// Enables resource owner password flow (via /connect/token, disabled by default as not secure)
        /// </summary>
        public bool IsPasswordFlowAllowed { get; set; }

        /// <summary>
        /// Enables device code flow (via /connect/verify, disabled by default since it's not common)
        /// </summary>
        public bool IsDeviceCodeFlowAllowed { get; set; }

        /// <summary>
        /// Disables logout endpoint (/connect/logout is enabled by default)
        /// </summary>
        public bool IsLogoutEndpointDisabled { get; set; }

        /// <summary>
        /// Disables authorization code flow (/connect/authorize is enabled by default)
        /// </summary>
        public bool IsAuthorizeFlowDisabled { get; set; }

        /// <summary>
        /// Disables refresh token flow (via /connect/token, enabled by default)
        /// </summary>
        public bool IsRefreshTokenFlowDisabled { get; set; }

        /// <summary>
        /// Determines whether to call options.RegisterScopes with available client scopes (enabled by default) or not
        /// </summary>
        public bool IsScopeRegistrationDisabled { get; set; }

        public OpenIddictServerBuilder OpenIddictServerBuilder { get; set; }
        public IConfiguration? Configuration { get; set; }

        /// <summary>
        /// Sets the Configuration section to configure OpenId clients
        /// </summary>
        /// <param name="configuration"></param>
        public OpenIddictSettings SetConfiguration(IConfiguration configuration)
        {
            Configuration = configuration;
            return this;
        }

        /// <summary>
        /// Sets the PublicUrl that will be prepended to relative URIs in 'RedirectUris' and 'PostLogoutRedirectUris'
        /// </summary>
        public OpenIddictSettings SetPublicUrl(string publicUrl)
        {
            PublicUrl = publicUrl;
            return this;
        }

        /// <summary>
        /// Enables resource owner password flow (via /connect/token, disabled by default as not secure)
        /// </summary>
        public OpenIddictSettings AllowPasswordFlow()
        {
            IsPasswordFlowAllowed = true;
            return this;
        }

        /// <summary>
        /// Enables device code flow (via /connect/verify, disabled by default since it's not common)
        /// </summary>
        public OpenIddictSettings AllowDeviceCodeFlow()
        {
            IsDeviceCodeFlowAllowed = true;
            return this;
        }
    }
}
