using AspNet.Security.OpenIdConnect.Primitives;
using Microsoft.AspNetCore.Identity;
using OpenIddict.Validation.AspNetCore;
using OpenIddictSetUp.Configs;
using OpenIddictSetUp.Context;
using OpenIddictSetUp.Entities;
using OpenIddictSetUp.OpenIddict;
using System.Security.Cryptography.X509Certificates;

namespace OpenIddictSetUp
{
    public static partial class Startup
    {
        public static void OpenIddictSeeder(this WebApplication app)
        {
            Task.Run(app.SeedOpenIddictClient).GetAwaiter().GetResult();
        }
        public static void ConfigureAuth(this WebApplicationBuilder builder)
        {
            var authSettings = new AuthSettings();
            builder.Configuration.Bind(nameof(AuthSettings), authSettings);
            builder.Services.AddSingleton(authSettings);
            builder.Services.Configure<IdentityOptions>(options =>
            {
                options.ClaimsIdentity.UserNameClaimType = OpenIdConnectConstants.Claims.Name;
                options.ClaimsIdentity.UserIdClaimType = OpenIdConnectConstants.Claims.Subject;
                options.ClaimsIdentity.RoleClaimType = OpenIdConnectConstants.Claims.Role;
                options.ClaimsIdentity.EmailClaimType = OpenIdConnectConstants.Claims.Email;
            });

            var tokenExpiry = TimeSpan.FromMinutes(authSettings.TokenExpiry);

            var publicUrl = builder.Configuration.GetSection("Auth").GetValue<string>("PublicHost");

            builder.Services.AddOpenIddict()
                .AddDefaultAuthorizationController(builder,
                options => options
                            .SetConfiguration(builder.Configuration.GetSection("openId"))
                            .SetPublicUrl(publicUrl)
                            .AllowDeviceCodeFlow()
                            .AllowPasswordFlow())
                .AddCore(options =>
                {
                    options.UseEntityFrameworkCore()
                    .UseDbContext<AppDbContext>()
                    .ReplaceDefaultEntities<AppOpenIddictApplication, AppOpenIddictAuthorization, AppOpenIddictScope, AppOpenIddictToken, Guid>();
                })
                
                .AddServer(async options =>
                {
                    options.DisableAccessTokenEncryption();

                    if(builder.Environment.IsDevelopment())
                    {
                        options.UseAspNetCore().DisableTransportSecurityRequirement();
                        options.AddDevelopmentEncryptionCertificate()
                             .AddDevelopmentSigningCertificate();
                    }
                    else
                    {
                        var certPwd = builder.Configuration.GetSection("OpenId:FileCertificate").GetValue<string>("Password");
                        byte[] rawData = await OpenIddictSetupExtensions.GetCertificate(builder);

                        options.AddSigningCertificate(new MemoryStream(rawData), certPwd, X509KeyStorageFlags.MachineKeySet |
                       X509KeyStorageFlags.Exportable);
                    }
                })

                // Register the OpenIddict validation components.
                .AddValidation(options =>
                {
                    // Import the configuration from the local OpenIddict server instance.
                    options.UseLocalServer();

                    // Register the ASP.NET Core host.
                    options.UseAspNetCore();
                });

            // if you want to secure some controllers/actions within the same project with JWT
            // you need to configure something like the following
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme =
                    OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme =
                    OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
            });

            builder.Services.AddAuthorization();

        }
    }
}
