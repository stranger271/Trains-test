using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OAuth;
using Owin;
using System;
using Trains.Models;
using Trains.Providers;

[assembly: OwinStartup(typeof(Trains.Startup))]
namespace Trains
{
    public class Startup
    {

        public static OAuthAuthorizationServerOptions OAuthOptions { get; private set; }

        public static string PublicClientId { get; private set; }

        public void Configuration(IAppBuilder app)
        {

            app.CreatePerOwinContext<TrainsContext>(TrainsContext.Create);
            app.CreatePerOwinContext<TrainsUserManager>(TrainsUserManager.Create);

            app.UseCookieAuthentication(new CookieAuthenticationOptions());
            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);
                  
            PublicClientId = "MyId";

            OAuthOptions = new OAuthAuthorizationServerOptions
            {
                TokenEndpointPath = new PathString("/Token"),
                Provider = new ApplicationOAuthProvider(PublicClientId),
               // AuthorizeEndpointPath = new PathString("/api/Account/ExternalLogin"),
                AccessTokenExpireTimeSpan = TimeSpan.FromDays(1),

                //TODO В рабочем режиме задайте AllowInsecureHttp = false
                AllowInsecureHttp = true
            };


            app.UseOAuthBearerTokens(OAuthOptions);


        }
    }
}