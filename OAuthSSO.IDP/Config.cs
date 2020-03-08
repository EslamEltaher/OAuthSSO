using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Test;

namespace OAuthSSO.IDP
{
    public class Config
    {
        internal static List<TestUser> GetUsers()
        {
            return new List<TestUser>() {
                new TestUser
                {
                    SubjectId = "d860efca-22d9-47fd-8249-791ba61b07c7",
                    Username = "Frank",
                    Password = "password",

                    Claims = new List<Claim>
                    {
                        new Claim("given_name", "Frank"),
                        new Claim("family_name", "Underwood"),
                        new Claim("address", "1, Main Street"),
                        new Claim("role", "FreeUser"),
                        new Claim("subscriptionlevel", "FreeUser"),
                        new Claim("country", "us")
                    }
                },
                new TestUser
                {
                    SubjectId = "b7539694-97e7-4dfe-84da-b4256e1ff5c7",
                    Username = "Claire",
                    Password = "password",

                    Claims = new List<Claim>
                    {
                        new Claim("given_name", "Claire"),
                        new Claim("family_name", "Underwood"),
                        new Claim("address", "2, Big Street"),
                        new Claim("role", "PayingUser"),
                        new Claim("subscriptionlevel", "PayingUser"),
                        new Claim("country", "be")
                    }
                }
            };
        }

        internal static IEnumerable<Client> GetClients()
        {
            return new List<Client>() {
                new Client() {
                    ClientId = "imagegalleryclient",
                    ClientName = "Image Gallery",
                    ClientSecrets = { new Secret("abcdefghijklmnopqrstuvwxyz".Sha256()) },
                    AllowedGrantTypes = GrantTypes.Hybrid,
                    RedirectUris = {
                        "https://localhost:44357/signin-oidc"
                    },
                    AllowedScopes = {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Address,
                        "roles",
                        "imagegalleryapi",
                        "country",
                        "subscriptionlevel"
                    },
                    //to make the UserClaims available in IdToken we can set:
                        //AlwaysIncludeUserClaimsInIdToken = true           
                        
                    //Logging out
                    PostLogoutRedirectUris = {
                        "https://localhost:44357/signout-callback-oidc"
                    }
                }
            };
        }

        internal static IEnumerable<IdentityResource> GetResources()
        {
            return new List<IdentityResource>() {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResources.Address(),
                new IdentityResource("roles", "Your role(s)", new List<string> {"role"}),
                new IdentityResource("country", "The Country you're living in", new List<string>{ "country" }),
                new IdentityResource("subscriptionlevel", "Your Subscription Level", new List<string>{ "subscriptionlevel" }),
            };
        }

        internal static IEnumerable<ApiResource> GetApiResources()
        {
            return new List<ApiResource>() {
                new ApiResource("imagegalleryapi", "Image Gallery API", new List<string>() { "role" })
            };
        }
    }
}
