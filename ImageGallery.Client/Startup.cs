using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using ImageGallery.Client.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Linq;

namespace ImageGallery.Client
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc();

            // register an IHttpContextAccessor so we can access the current
            // HttpContext in services by injecting it
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // register an IImageGalleryHttpClient
            services.AddScoped<IImageGalleryHttpClient, ImageGalleryHttpClient>();

            services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options => {})
                .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options => {
                    //options.AuthenticationScheme = "oidc";
                    options.Authority = "https://localhost:44356/";
                    options.RequireHttpsMetadata = true;
                    options.ClientId = "imagegalleryclient";
                    options.Scope.Add("openid");
                    options.Scope.Add("profile");
                    options.Scope.Add("address");
                    options.ResponseType = "code id_token";
                    options.CallbackPath = "/signin-oidc"; //signin-oidc is the default value
                    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.SaveTokens = true;
                    options.ClientSecret = "abcdefghijklmnopqrstuvwxyz";
                    //options.SignedOutCallbackPath = "/signout-callback-oidc"; //signout-callback-oidc is the default value

                    //instead of making the IDP return the User claims in the Id Token 
                    //we to make the client application hit the (UserInfo) Endpoint so we set:
                    options.GetClaimsFromUserInfoEndpoint = true;

                    //34- Keeping Only claims that we need
                    options.Events = new OpenIdConnectEvents()
                    {
                        OnTokenValidated = async tokenValidatedContext => {
                            var claimsIdentity = tokenValidatedContext.Principal.Identity as ClaimsIdentity;
                            var subjectClaim = claimsIdentity.Claims.FirstOrDefault(c => c.Type == "sub");

                            //or 

                            subjectClaim = tokenValidatedContext.Principal.Claims.FirstOrDefault(c => c.Type == "sub");


                            var newClaimsIdentity = new ClaimsIdentity(tokenValidatedContext.Scheme.Name, "given_name", "role");
                            newClaimsIdentity.AddClaim(subjectClaim);

                            tokenValidatedContext.Principal = new ClaimsPrincipal(newClaimsIdentity);
                        },
                        OnUserInformationReceived = async userInformationReceivedContext => {
                            userInformationReceivedContext.User.Remove("address");
                            await System.Threading.Tasks.Task.CompletedTask;
                        }
                    };
                });

            services.AddAuthorization();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                //app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Shared/Error");
            }
            
            app.UseStaticFiles();

            app.Use(async (req, res) => {
                var url = req.Request.Path.ToString();
                await res();
            });

            //33- to keep the originical claim Types mapping we clear the default mapper
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            app.UseAuthentication();
            //app.UseAuthorization();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Gallery}/{action=Index}/{id?}");
            });
        }         
    }
}
