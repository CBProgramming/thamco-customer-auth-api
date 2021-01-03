using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Logging;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ThAmCo.Data;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using ThAmCo.Repo;

namespace CustomerAuthServer
{
    public class Startup
    {
        public Startup(IConfiguration configuration, Microsoft.AspNetCore.Hosting.IHostingEnvironment env)
        {
            Configuration = configuration;
            Env = env;
        }

        private IConfiguration Configuration { get; }
        private Microsoft.AspNetCore.Hosting.IHostingEnvironment Env { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAutoMapper(typeof(Startup));
            // configure EF context to use for storing Identity account data
            services.AddDbContext<AccountDbContext>(options => options.UseSqlServer(
                Configuration.GetConnectionString("AccountConnection"), optionsBuilder =>
                {
                    optionsBuilder.EnableRetryOnFailure(10, TimeSpan.FromSeconds(10), null);
                }));

            // configure Identity account management
            services.AddIdentity<AppUser, AppRole>()
                    .AddEntityFrameworkStores<AccountDbContext>()
                    .AddDefaultTokenProviders();
            IdentityModelEventSource.ShowPII = true;

            // add bespoke factory to translate our AppUser into claims
            services.AddScoped<IUserClaimsPrincipalFactory<AppUser>, AppClaimsPrincipalFactory>();
            services.AddScoped<IRepository, Repository>();

            // configure Identity security options
            services.Configure<IdentityOptions>(options =>
            {
                // Password settings
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequiredUniqueChars = 6;

                // Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                // User settings
                options.User.RequireUniqueEmail = true;

                // Sign-in settings
                options.SignIn.RequireConfirmedEmail = false;
            });

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            if (Env.IsDevelopment())
            {
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultForbidScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer()
                .AddJwtBearer("customer_web_app", options =>
                {
                    options.Audience = "customer_auth_customer_api";
                    options.Authority = "https://localhost:43389";
                })
                .AddJwtBearer("customer_account_api", options =>
                {
                    options.Audience = "customer_auth_staff_api";
                    options.Authority = "https://localhost:43389";
                });
            }
            else
            {
                services.AddAuthentication()
                .AddJwtBearer("customer_web_app", options =>
                {
                    options.Audience = "customer_auth_customer_api";
                    options.Authority = "https://customerauththamco.azurewebsites.net";
                })
                .AddJwtBearer("customer_account_api", options =>
                {
                    options.Audience = "customer_auth_staff_api";
                    options.Authority = "https://customerauththamco.azurewebsites.net";
                });
            }


            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            // configure IdentityServer (provides OpenId Connect and OAuth2)
            if (Env.IsDevelopment())
            {
                services.AddIdentityServer()
                    .AddInMemoryIdentityResources(Configuration.GetIdentityResources())
                    .AddInMemoryApiResources(Configuration.GetIdentityApis())
                    .AddInMemoryClients(Configuration.GetIdentityClients())
                    .AddAspNetIdentity<AppUser>()
                    .AddDeveloperSigningCredential();
            }
            else
            {
                services.AddIdentityServer()
                    .AddInMemoryIdentityResources(Configuration.GetIdentityResources())
                    .AddInMemoryApiResources(Configuration.GetIdentityApis())
                    .AddInMemoryClients(Configuration.GetIdentityClients())
                    .AddAspNetIdentity<AppUser>()
                    .AddDeveloperSigningCredential();
            }

            // TODO: developer signing cert above should be replaced with a real one
            // TODO: should use AddOperationalStore to persist tokens between app executions
        }

        public void Configure(IApplicationBuilder app, Microsoft.AspNetCore.Hosting.IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                //app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseStaticFiles();

            app.UseCookiePolicy();

            app.UseAuthentication();

            app.UseAuthorization();

            // use IdentityServer middleware during HTTP requests
            app.UseIdentityServer();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            //app.UseMvc();
        }
    }
}