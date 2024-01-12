using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using OpenIddictSetUp.Configs;
using OpenIddictSetUp.Entities;
using OpenIddictSetUp.OpenIddict;
using System.Security.Claims;
using System.Security;
using static OpenIddict.Abstractions.OpenIddictConstants;
using OpenIddictSetUp.Contract.Abstraction;
using OpenIddictSetUp.ViewModels;
using OpenIddictSetUp.Enums;
using IdentityModel;

namespace OpenIddictSetUp.Controllers
{
    [Route("api")]
    [ApiController]
    public class AuthorizationController : ControllerBase
    {
        private readonly AuthSettings _authSettings;

        protected readonly SignInManager<AppUser> _signInManager;
        protected readonly UserManager<AppUser> _userManager;
        private readonly ILogger<AuthorizationController> _logger;
        private readonly IOpenIddictClientConfigurationProvider _clientConfigurationProvider;

        public AuthorizationController(IOpenIddictClientConfigurationProvider clientConfigurationProvider, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IMemoryCacheService cacheService, AuthSettings authSettings, ILogger<AuthorizationController> logger)
        {
            _clientConfigurationProvider = clientConfigurationProvider;
            _userManager = userManager;
            _signInManager = signInManager;
            _cacheService = cacheService;
            _authSettings = authSettings;
            _logger = logger;
        }

        private readonly IMemoryCacheService _cacheService;

        protected virtual string ControllerName => GetType().Name.Replace("Controller", "");

        /// <summary>
        ///  Customized error that is returned in case of authentication error
        /// </summary>
        protected virtual IActionResult Error(string description)
        {
            var properties = new AuthenticationProperties(
                new Dictionary<string, string>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = description
                }!
            );

            return Forbid(properties, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        /// <summary>
        /// Default error that is returned in case of authentication error
        /// </summary>
        protected virtual IActionResult StandardError()
        {
            return Forbid(authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                     properties: new AuthenticationProperties(new Dictionary<string, string>
                     {
                         [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                         [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "Invalid email or password."
                     }));
        }

        /// <summary>
        /// Returns the ActionResult that is later converted by OpenIddict into a JWT token.
        /// Sets up accesstoken/refreshtoken timeouts, add claims to the tokens.
        /// </summary>

        private async Task CacheUserPermission(string roleName, Claim[] permissionClaims)
        {
            await _cacheService.Clear(roleName);

            var permissions = from pc in permissionClaims
                              select new PermissionViewModel
                              {
                                  Name = pc.Type,
                                  Id = pc.Value
                              };

            await _cacheService.SetValueAsync(roleName, permissions);
        }

        protected virtual async Task<IActionResult> SignInUser(AppUser user, OpenIddictRequest? request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var principal = await _signInManager.CreateUserPrincipalAsync(user);
            var identity = principal.Identity as ClaimsIdentity;
            var permissionClaims = (from c in principal.Claims
                                    where c.Type == nameof(Permission)
                                    select c).ToArray();

            Array.ForEach(permissionClaims, identity.RemoveClaim);

            var roleName = principal.FindFirst(Claims.Role)?.Value;

            if (roleName != null)
                await CacheUserPermission(roleName, permissionClaims);

            if (!string.IsNullOrEmpty(request.ClientId) && _clientConfigurationProvider.TryGetConfiguration(
                    request.ClientId, out var configuration))
            {
                if (configuration.RefreshTokenLifetime != null)
                {
                    principal.SetRefreshTokenLifetime(TimeSpan.FromSeconds(configuration.RefreshTokenLifetime.Value));
                }

                if (configuration.AccessTokenLifetime != null)
                {
                    principal.SetAccessTokenLifetime(TimeSpan.FromSeconds(configuration.AccessTokenLifetime.Value));
                }
            }

            await AddClaims(principal, user, request);

            var scopes = request.GetScopes();
            principal.SetScopes(scopes);

            _logger.LogInformation("New token created for user {UserId}, scopes: {scopes}",
                user.Id, string.Join(", ", scopes));

            foreach (var claim in principal.Claims)
            {
                claim.SetDestinations(GetDestinations(claim, principal));
            }

            //if (_authSettings.Audiences.Any())
            //    principal.SetAudiences(_authSettings.Audiences);

            if (!await _signInManager.CanSignInAsync(user))
            {
                return Error("signin_requirements_not_met");
            }

            return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        /// <summary>
        /// Implements logout endpoint
        /// </summary>
        /// <returns></returns>
        [HttpGet("~/connect/logout")]
        [AllowAnonymous]
        public virtual async Task<IActionResult> Logout()
        {
            // Ask ASP.NET Core Identity to delete the local and external cookies created
            // when the user agent is redirected from the external identity provider
            // after a successful authentication flow (e.g Google or Facebook).
            await _signInManager.SignOutAsync();

            // Returning a SignOutResult will ask OpenIddict to redirect the user agent
            // to the post_logout_redirect_uri specified by the client application or to
            // the RedirectUri specified in the authentication properties if none was set.
            return SignOut(authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties { RedirectUri = "/" });
        }


        /// <summary>
        /// Implements token endpoint for all auth flows
        /// </summary>
        [AllowAnonymous]
        [HttpPost("~/connect/token"), Produces("application/json")]
        public virtual async Task<IActionResult> Exchange()
        {
            OpenIddictRequest? request = HttpContext.GetOpenIddictServerRequest();

            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (request.IsAuthorizationCodeGrantType())
            {
                var authenticateResult = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

                if (!authenticateResult.Succeeded)
                {
                    return StandardError();
                }

                var user = await _userManager.GetUserAsync(authenticateResult.Principal);

                return await SignInUser(user, request);
            }
            else if (request.IsPasswordGrantType())
            {
                var user = await _userManager.FindByEmailAsync(request.Username)
                     ?? await _userManager.FindByNameAsync(request.Username);

                if (user == null)
                {
                    return StandardError();
                }


                if (_userManager.SupportsUserLockout)
                {
                    await _userManager.ResetAccessFailedCountAsync(user);
                }

                // Validate the username/password parameters and ensure the account is not locked out.
                var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password,
                    lockoutOnFailure: true);

                if (!result.Succeeded)
                {
                    return StandardError();
                }


                if (!await _signInManager.CanSignInAsync(user))
                {
                    return Forbid(authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                       properties: new AuthenticationProperties(new Dictionary<string, string>
                       {
                           [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                           [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "You are not allowed to sign in."
                       }));
                }

                await _userManager.UpdateAsync(user);
                return await SignInUser(user, request);
            }
            else if (request.IsRefreshTokenGrantType() || request.IsDeviceCodeGrantType())
            {
                // Retrieve the claims principal stored in the refresh token.
                var info = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

                // Retrieve the user profile corresponding to the refresh token.
                // Note: if you want to automatically invalidate the refresh token
                // when the user password/roles change, use the following line instead:
                // var user = _signInManager.ValidateSecurityStampAsync(info.Principal);
                var user = await _userManager.GetUserAsync(info.Principal);
                if (user == null)
                {
                    if (request.IsRefreshTokenGrantType())
                    {
                        return Error("refresh_token_invalid");
                    }
                    else if (request.IsDeviceCodeGrantType())
                    {
                        return Error("device_code_invalid");
                    }
                    else
                    {
                        return Error("token_invalid");
                    }
                }

                // Ensure the user is still allowed to sign in.
                if (!await _signInManager.CanSignInAsync(user))
                {
                    return StandardError();
                }

                return await SignInUser(user, request);
            }

            throw new NotImplementedException("The specified grant type is not implemented.");
        }

        /// <summary>
        /// Returns destinations to which a certain claim could be returned
        /// </summary>
        protected virtual IEnumerable<string> GetDestinations(Claim claim, ClaimsPrincipal principal)
        {
            // Note: by default, claims are NOT automatically included in the access and identity tokens.
            // To allow OpenIddict to serialize them, you must attach them a destination, that specifies
            // whether they should be included in access tokens, in identity tokens or in both.
            switch (claim.Type)
            {
                case Claims.Name:
                    yield return Destinations.AccessToken;

                    if (principal.HasScope(Scopes.Profile))
                        yield return Destinations.IdentityToken;

                    yield break;

                case Claims.Email:
                    yield return Destinations.AccessToken;

                    if (principal.HasScope(Scopes.Email))
                        yield return Destinations.IdentityToken;

                    yield break;

                case Claims.Role:
                    yield return Destinations.AccessToken;

                    if (principal.HasScope(Scopes.Roles))
                        yield return Destinations.IdentityToken;

                    yield break;

                // Never include the security stamp in the access and identity tokens, as it's a secret value.
                case "AspNet.Identity.SecurityStamp":
                    yield break;

                default:
                    yield return Destinations.AccessToken;
                    yield break;
            }
        }

        /// <summary>
        /// Adds claims from <param name="user"/> to <param name="principal"/>.
        /// Override this function if you want to remove/modify some pre-added claims.
        /// If you just want to add more claims, consider overriding <see cref="GetClaims"/>
        /// </summary>
        protected virtual async Task AddClaims(ClaimsPrincipal principal, AppUser user, OpenIddictRequest openIddictRequest)
        {
            IList<Claim> claims = await GetClaims(user, openIddictRequest);

            ClaimsIdentity claimIdentity = principal.Identities.First();
            claimIdentity.AddClaims(claims);
        }

        /// <summary>
        /// Returns claims that will be added to the user's principal (and later to JWT token).
        /// Consider overriding this function if you want to add more claims.
        /// </summary>
        protected virtual Task<IList<Claim>> GetClaims(AppUser user, OpenIddictRequest openIddictRequest)
        {
            return Task.FromResult(new List<Claim>()
                {
                new(JwtClaimTypes.NickName, user.UserName),
                new(JwtClaimTypes.Id, user.Id.ToString() ?? string.Empty),
                new(JwtClaimTypes.Subject, user.Id.ToString() ?? string.Empty),
                } as IList<Claim>
            );
        }
    }
}
