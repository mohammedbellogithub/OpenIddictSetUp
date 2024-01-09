using AspNet.Security.OpenIdConnect.Primitives;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using OpenIddictSetUp.Context;
using OpenIddictSetUp.Entities;
using OpenIddictSetUp.OpenIddict;

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
            builder.Services.Configure<IdentityOptions>(options =>
            {
                options.ClaimsIdentity.UserNameClaimType = OpenIdConnectConstants.Claims.Name;
                options.ClaimsIdentity.UserIdClaimType = OpenIdConnectConstants.Claims.Subject;
                options.ClaimsIdentity.RoleClaimType = OpenIdConnectConstants.Claims.Role;
                options.ClaimsIdentity.EmailClaimType = OpenIdConnectConstants.Claims.Email;
            });

            //tokenExpiry
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
                        options.
                    }
                });


        }
    }
}
