using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace ImageGallery.Client.Services
{
    public class ImageGalleryHttpClient : IImageGalleryHttpClient
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private HttpClient _httpClient = new HttpClient();

        public ImageGalleryHttpClient(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        
        public async Task<HttpClient> GetClient()
        {
            var expires_at = await _httpContextAccessor.HttpContext.GetTokenAsync("expires_at");
            bool willExpire = string.IsNullOrEmpty(expires_at)
                || (DateTime.Parse(expires_at).AddSeconds(-60)).ToUniversalTime() < DateTime.UtcNow;


            string access_token = willExpire ? 
                await RenewTokens()
                : await _httpContextAccessor.HttpContext
                    .GetTokenAsync(OpenIdConnectParameterNames.AccessToken);

            if(!string.IsNullOrEmpty(access_token))
            {
                _httpClient.SetBearerToken(access_token);
            }

            _httpClient.BaseAddress = new Uri("https://localhost:44346/");
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            return _httpClient;
        }

        private async Task<string> RenewTokens()
        {
            var discoveryClient = new DiscoveryClient("https://localhost:44356/");
            var metadataResponse = await discoveryClient.GetAsync();

            var refresh_token = await _httpContextAccessor.HttpContext.GetTokenAsync(OpenIdConnectParameterNames.RefreshToken);

            var tokenClient = new TokenClient(metadataResponse.TokenEndpoint, "imagegalleryclient", "abcdefghijklmnopqrstuvwxyz");
            var tokenResult = await tokenClient.RequestRefreshTokenAsync(refresh_token);

            if (!tokenResult.IsError)
            {
                var authenticationInfo = await _httpContextAccessor.HttpContext
                    .AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                var tokens = authenticationInfo.Properties.GetTokens();

                string expires_at = (DateTime.UtcNow + TimeSpan.FromSeconds(tokenResult.ExpiresIn))
                    .ToString("o", System.Globalization.CultureInfo.InvariantCulture);

                authenticationInfo.Properties.UpdateTokenValue(OpenIdConnectParameterNames.AccessToken, tokenResult.AccessToken);
                authenticationInfo.Properties.UpdateTokenValue(OpenIdConnectParameterNames.RefreshToken, tokenResult.RefreshToken);
                authenticationInfo.Properties.UpdateTokenValue("expires_at", expires_at);

                await _httpContextAccessor.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, 
                    authenticationInfo.Principal, 
                    authenticationInfo.Properties);

                return tokenResult.AccessToken;
            }
            else
            {
                throw new Exception("Error in renewing access token");
            }
        }
    }
}

