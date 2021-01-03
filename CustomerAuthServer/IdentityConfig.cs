using IdentityModel;
using IdentityServer4.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CustomerAuthServer
{
    public static class IdentityConfigurationExtensions
    {
        public static IEnumerable<IdentityResource> GetIdentityResources(this IConfiguration configuration)
        {
            return new IdentityResource[]
            {
                new IdentityResources.OpenId(),

                new IdentityResources.Profile(),

                new IdentityResource(name: "roles", claimTypes: new[] { "role" }),

                new IdentityResource(name: "id", claimTypes: new[] { "id" }),

                //new IdentityResource(name: "id", claimTypes: new[] { "id" }),

                //new IdentityResource(JwtClaimTypes.Id, claimTypes: new[] { "id" }),

                //new IdentityResource("CustomerId", new [] {"CustomerId"})
            };
        }

        public static IEnumerable<ApiResource> GetIdentityApis(this IConfiguration configuration)
        {
            return new ApiResource[]
            {
                new ApiResource("customer_auth_customer_api", "ThAmCo Customer Account Customer Management")
                {
                    UserClaims = {"name","role","id", JwtClaimTypes.Id }
                },

                new ApiResource("customer_auth_staff_api", "ThAmCo Customer Account Staff Management")
                {
                    UserClaims = {"name","role","id", JwtClaimTypes.Id }
                },

                new ApiResource("review_api","Customer Ratings API")
                {
                    UserClaims = {"name","role","id"}
                },

                new ApiResource("customer_account_api","Customer Account API")
                {
                    UserClaims = {"name","role", "id", JwtClaimTypes.Id }
                },

                new ApiResource("customer_product_api","Customer Products API")
                {
                    UserClaims = {"name","role", "id" }
                },

                new ApiResource("customer_ordering_api","Customer Orders API")
                {
                    UserClaims = {"name","role", "id" }
                },

                new ApiResource("staff_product_api","Staff Products API")
                {
                    UserClaims = {"name","role", "id" }
                },

                 new ApiResource("customer_web_app","Customer Web App")
                {
                    UserClaims = {"name","role", "id", JwtClaimTypes.Id }
                 },

                new ApiResource("invoice_api","Invoices API")
                {
                    UserClaims = {"name","role", "id" }
                }
            };
        }

        public static IEnumerable<Client> GetIdentityClients(this IConfiguration configuration)
        {
            var clients = new Client[]
            {
                new Client
                {
                    ClientId = "customer_web_app",
                    ClientName = "Customer Web App",
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPasswordAndClientCredentials,
                    ClientSecrets =
                    {
                        new Secret(configuration.GetValue<string>("CustomerWebAppPassword"))
                    },
                    AllowedScopes =
                    {
                        //auth server access
                        "customer_auth_customer_api",
                        "customer_auth_staff_api",

                        //customer service apis
                        "review_api",
                        "customer_account_api",
                        "customer_product_api",
                        "customer_ordering_api",

                        //for sign in
                        "openid",
                        "profile",
                        "roles",
                        "id",
                        JwtClaimTypes.Id
                    },
                    RequireConsent = false

                },
                new Client
                {
                    ClientId = "review_api",
                    ClientName = "Customer Ratings API",
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    ClientSecrets =
                    {
                        new Secret(configuration.GetValue<string>("ReviewApiPassword"))
                    },
                    AllowedScopes =
                    {
                        "roles",
                        "id"
                        //auth server access
                        //"thamco_account_api"
                    },
                    RequireConsent = false
                },
                new Client
                {
                    ClientId = "customer_account_api",
                    ClientName = "Customer Account API",
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    ClientSecrets =
                    {
                        new Secret(configuration.GetValue<string>("CustomerAccountApiPassword"))
                    },
                    AllowedScopes =
                    {
                        "roles",
                        "id",
                        JwtClaimTypes.Id,
                        "customer_ordering_api",
                        "customer_auth_customer_api",
                        "customer_auth_staff_api",
                        "review_api"
                    },
                    RequireConsent = false
                },
                new Client
                {
                    ClientId = "customer_product_api",
                    ClientName = "Customer Products API",
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    ClientSecrets =
                    {
                        new Secret(configuration.GetValue<string>("CustomerProductApiPassword"))
                    },
                    AllowedScopes =
                    {
                        "roles",
                        "id",
                        "customer_ordering_api"
                        //auth server access
                        //"thamco_account_api"
                    },
                    RequireConsent = false
                },
                new Client
                {
                    ClientId = "customer_ordering_api",
                    ClientName = "Customer Orders API",
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    ClientSecrets =
                    {
                        new Secret(configuration.GetValue<string>("CustomerOrderingApiPassword"))
                    },
                    AllowedScopes =
                    {
                        "roles",
                        "id",
                        //auth server access
                        //"thamco_account_api",

                        "customer_auth_customer_api",
                        "customer_account_api",
                        "invoice_api",
                        "staff_product_api",
                        "review_api"

                        //"thamco_account_api"
                    },
                    RequireConsent = false
                },
            };
            return clients;
        }
    }
}