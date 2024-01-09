using Microsoft.Extensions.DependencyInjection;
using System.Security.Cryptography.X509Certificates;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace OpenIddictSetUp.OpenIddict
{
    public static class OpenIddictExtensions
    {
        /// <summary>
        /// Configures openiddict with signing certificate
        /// </summary>
        public static OpenIddictServerBuilder AddSigningCertificate(
            this OpenIddictServerBuilder options,
            OpenIdCertificateInfo signingCertificate
        )
        {
            if (
                !string.IsNullOrEmpty(signingCertificate?.Password)
                && !string.IsNullOrEmpty(signingCertificate?.Base64Certificate)
            )
            {
                options.AddSigningCertificate(
                    new MemoryStream(Convert.FromBase64String(signingCertificate.Base64Certificate)),
                    signingCertificate.Password
                );
            }
            else
            {
                options.AddDevelopmentSigningCertificate();
            }

            return options;
        }

        ///// <summary>
        ///// Configures openiddict with encryption certificate from appsettings.json
        ///// </summary>
        //public static OpenIddictServerBuilder AddEncryptionCertificateFromConfiguration(
        //    this OpenIddictServerBuilder options,
        //    IConfiguration configuration
        //)
        //{
        //    OpenIdCertificateInfo encryptionCertificate = configuration.Get<OpenIdCertificateInfo>();
        //    return options.AddEncryptionCertificate(encryptionCertificate);
        //}

        /// <summary>
        /// Configures openiddict with encryption certificate
        /// </summary>
        public static OpenIddictServerBuilder AddEncryptionCertificate(
            this OpenIddictServerBuilder options,
            byte[] encryptionCertificate, string password)
        {
            if (encryptionCertificate != null && encryptionCertificate.Any())
            {
                options.AddEncryptionCertificate(new MemoryStream(encryptionCertificate), password, X509KeyStorageFlags.MachineKeySet |
                        X509KeyStorageFlags.Exportable);
            }
            else
            {
                options.AddDevelopmentEncryptionCertificate();
            }

            return options;
        }

        /// <summary>
        /// Registers implementation of IOption&lt;OpenIddictConfiguration&gt; and IOpenIddictClientConfigurationProvider
        /// </summary>
        internal static OpenIddictBuilder AddOpenIddictConfiguration(
            this OpenIddictBuilder openIddictBuilder,
            IConfiguration configuration
        )
        {
            IServiceCollection services = openIddictBuilder.Services;
            services.AddTransient<IOpenIddictClientConfigurationProvider, OpenIddictClientConfigurationProvider>();
            services.Configure<OpenIddictConfiguration>(configuration);

            return openIddictBuilder;
        }


        /// <summary>
        /// Configures OpenIddict to use Token and Iposweb.Coreorization endpoints
        /// </summary>
        public static OpenIddictBuilder AddDefaultAuthorizationController(
            this OpenIddictBuilder openIddictBuilder,
            WebApplicationBuilder builder,
            Action<OpenIddictSettings> configuration = null)
        {
            return openIddictBuilder
                .AddServer(options =>
                {
                    var settings = new OpenIddictSettings(options);
                    configuration?.Invoke(settings);

                    if (settings.Configuration != null)
                    {
                        openIddictBuilder.AddOpenIddictConfiguration(settings.Configuration);
                        var typedConfiguration = settings.Configuration.Get<OpenIddictConfiguration>();

                        if (!builder.Environment.IsDevelopment())
                        {
                            byte[] rawData = GetCertificate(builder).GetAwaiter().GetResult();
                            options.AddEncryptionCertificate(rawData,
                               typedConfiguration.FileCertificate.Password);
                        }
                        else
                        {
                            options.AddDevelopmentEncryptionCertificate();
                        }

                        if (typedConfiguration?.ClientConfiguration != null && typedConfiguration.ClientConfiguration.Any())
                        {
                            options.Services.AddSingleton<IPublicUrlProvider>(
                                new PublicUrlProvider(
                                    !string.IsNullOrEmpty(settings.PublicUrl)
                                        ? settings.PublicUrl
                                        : typedConfiguration.PublicUrl
                                )
                            );

                            if (!settings.IsScopeRegistrationDisabled)
                            {
                                string[] scopes = typedConfiguration.ClientConfiguration
                                    .SelectMany(x => x.Value?.Permissions ?? new HashSet<string>())
                                    .Where(
                                        x => x.StartsWith(Permissions.Prefixes.Scope)
                                    )
                                    .Select(
                                        x =>
                                            x.Substring(Permissions.Prefixes.Scope.Length)
                                    )
                                    .ToArray();

                                options.RegisterScopes(scopes);
                            }
                        }
                    }

                    options.SetTokenEndpointUris("/connect/token");
                    options.SetIntrospectionEndpointUris("/connect/introspect");
                    options.UseAspNetCore().EnableTokenEndpointPassthrough();

                    if (!settings.IsLogoutEndpointDisabled)
                    {
                        options.SetLogoutEndpointUris("/connect/logout");
                        options.UseAspNetCore().EnableLogoutEndpointPassthrough();
                    }

                    if (!settings.IsAuthorizeFlowDisabled)
                    {
                        options.AllowAuthorizationCodeFlow()
                        .RequireProofKeyForCodeExchange()
                            .SetAuthorizationEndpointUris("/connect/authorize");

                        options.UseAspNetCore().EnableAuthorizationEndpointPassthrough();
                    }

                    if (settings.IsPasswordFlowAllowed)
                    {
                        options.AllowPasswordFlow();
                    }

                    if (settings.IsDeviceCodeFlowAllowed)
                    {
                        options.AllowDeviceCodeFlow().SetDeviceEndpointUris("/connect/device")
                            .SetVerificationEndpointUris("/connect/verify");
                        options.UseAspNetCore().EnableVerificationEndpointPassthrough();
                    }

                    if (!settings.IsRefreshTokenFlowDisabled)
                    {
                        options.AllowRefreshTokenFlow();
                    }

                    options.RegisterScopes(Scopes.Email, Scopes.Profile, Scopes.Roles, Scopes.OpenId);
                });
        }


    }
}
