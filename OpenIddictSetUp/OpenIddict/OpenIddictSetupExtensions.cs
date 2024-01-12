using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using OpenIddictSetUp.Configs;
using System.Security.Cryptography.X509Certificates;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace OpenIddictSetUp.OpenIddict
{
    public static class OpenIddictSetupExtensions
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
        /// Configures OpenIddict to use Token and app.Coreorization endpoints
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

        public static async Task<byte[]> GetCertificate(WebApplicationBuilder builder)
        {
            var certificate = new byte[] { };
            var certificateSettings = new CertificateSettings();
            builder.Configuration.Bind(nameof(CertificateSettings), certificateSettings);
            builder.Services.AddSingleton(certificateSettings);
            var storageAccount = CloudStorageAccount.Parse(certificateSettings.connectionString);
            var storageCredentials = new StorageCredentials(storageAccount.Credentials.AccountName, certificateSettings.accesskey);

            var cloudBlobContainer = new CloudBlobContainer(new Uri(certificateSettings.blobUrl), storageCredentials);
            var cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(certificateSettings.blobName);

            // Download the certificate
            using (var ms = new MemoryStream())
            {
                await cloudBlockBlob.DownloadToStreamAsync(ms);
                ms.Seek(0, SeekOrigin.Begin);

                certificate = ms.ToArray();
            }

            return certificate;
        }
    }
}
