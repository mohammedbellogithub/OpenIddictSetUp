using OpenIddict.EntityFrameworkCore.Models;

namespace OpenIddictSetUp.Entities
{
    public class OpenIddict
    {
    }
    public class AppOpenIddictApplication : OpenIddictEntityFrameworkCoreApplication<Guid, AppOpenIddictAuthorization, AppOpenIddictToken>
    {
        public AppOpenIddictApplication()
        {
            Id = Guid.NewGuid();
        }
        public string? AppId { get; set; }
        public string? Language { get; set; }
    }

    public class AppOpenIddictAuthorization : OpenIddictEntityFrameworkCoreAuthorization<Guid, AppOpenIddictApplication, AppOpenIddictToken> { }
    public class AppOpenIddictScope : OpenIddictEntityFrameworkCoreScope<Guid> { }
    public class AppOpenIddictToken : OpenIddictEntityFrameworkCoreToken<Guid, AppOpenIddictApplication, AppOpenIddictAuthorization> { }


}
