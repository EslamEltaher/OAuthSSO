using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Test;

namespace OAuthSSO.IDP
{
    public class Config
    {
        internal static List<TestUser> GetUsers()
        {
            return new List<TestUser>() { };
        }

        internal static IEnumerable<Client> GetClients()
        {
            return new List<Client>() {};
        }

        internal static IEnumerable<IdentityResource> GetResources()
        {
            return new List<IdentityResource>() {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
            };
        }
    }
}
