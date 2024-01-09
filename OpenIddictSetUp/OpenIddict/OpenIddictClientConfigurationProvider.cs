using Microsoft.Extensions.Options;

namespace OpenIddictSetUp.OpenIddict
{
    public interface IOpenIddictClientConfigurationProvider
    {
        OpenIddictClientConfiguration? GetConfiguration(string clientId);
        /// <summary>
        /// Returns configuration for passed clientId (and `true` as returnor null if client is not found. 
        /// </summary>
        bool TryGetConfiguration(string clientId, out OpenIddictClientConfiguration configuration);

        /// <summary>
        /// Returns configuration for all clients
        /// </summary>
        IList<OpenIddictClientConfiguration> GetAllConfigurations();
    }

    public class OpenIddictClientConfigurationProvider : IOpenIddictClientConfigurationProvider
    {
        private readonly Dictionary<string, OpenIddictClientConfiguration> _clientConfiguration;

        public OpenIddictClientConfigurationProvider(IOptions<OpenIddictConfiguration> clientConfiguration)
        {
            _clientConfiguration = clientConfiguration.Value.ClientConfiguration.Values.ToDictionary(x => x.ClientId);
        }

        public IList<OpenIddictClientConfiguration> GetAllConfigurations()
        {
            return _clientConfiguration.Values.ToList();    
        }

        public OpenIddictClientConfiguration? GetConfiguration(string clientId)
        {
           return _clientConfiguration[clientId];
        }

        public bool TryGetConfiguration(string clientId, out OpenIddictClientConfiguration configuration)
        {
            return _clientConfiguration.TryGetValue(clientId, out configuration);
        }
    }
}
