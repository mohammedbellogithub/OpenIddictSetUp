using OpenIddict.Core;
using OpenIddictSetUp.Entities;

namespace OpenIddictSetUp.OpenIddict
{
    public static class OpenIddictManager
    {
        public static async Task SeedOpenIddictClient(this WebApplication builder)
        {
            await using var scope = builder.Services.CreateAsyncScope();

            var serviceProvider = scope.ServiceProvider;
            var manager = serviceProvider.GetRequiredService<OpenIddictApplicationManager<AppOpenIddictApplication>>();
            var publicUrl = builder.Configuration.GetSection("OpenId").GetValue<string>("PublicUrl");
            var clientConfigurationProvider = serviceProvider.GetService<IOpenIddictClientConfigurationProvider>();

            var clients = clientConfigurationProvider.GetAllConfigurations();

            foreach(var client in clients)
            {
                if(!string.IsNullOrEmpty(publicUrl)) 
                {
                    var baseUrl = new Uri(publicUrl);

                    PrependBaseUriToRelativeUris(client.RedirectUris, baseUrl);
                    PrependBaseUriToRelativeUris(client.PostLogoutRedirectUris, baseUrl);

                }

                var clientObject = await manager.FindByClientIdAsync(client.ClientId!).ConfigureAwait(false);

                if(clientObject is null)
                {
                    await manager.CreateAsync(client).ConfigureAwait(false);
                }
                else
                {
                    if (string.IsNullOrEmpty(client.ClientType))
                    {
                        if(string.IsNullOrEmpty(client.ClientSecret))
                        {
                            client.ClientType = "public";
                        }
                        else
                        {
                            client.ClientType = "confidential";
                        }
                    }

                    await manager.PopulateAsync(clientObject, client).ConfigureAwait(false);
                    await manager.UpdateAsync(clientObject, client.ClientSecret ?? "").ConfigureAwait(false);
                }
            }
        }

        private static void PrependBaseUriToRelativeUris(HashSet<Uri> uris, Uri baseUri)
        {
            if (uris == null)
                return;

            List<Uri> relativeUris = uris.Where(x => !x.IsAbsoluteUri).ToList();
            foreach (var relativeUri in relativeUris)
            {
                uris.Remove(relativeUri);
                uris.Add(new Uri(baseUri, relativeUri));
            }
        }
    }
}
