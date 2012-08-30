using System.Web;
using Microsoft.IdentityModel.Web;

namespace SiteMonitR.Web.FederatedIdentity.Infrastructure
{
    public class CustomSessionAuthenticationModule : SessionAuthenticationModule
    {
        protected override void InitializeModule(HttpApplication context)
        {
            // shortcircuit registration of the module SessionAuthenticationModule events if fed auth is not configured
            var settings = new FederatedIdentityConfiguration();
            if (settings.AuthEnabled)
            {
                base.InitializeModule(context);
            }
        }

        protected override void InitializePropertiesFromConfiguration(string serviceName)
        {
            var settings = new FederatedIdentityConfiguration();

            this.CookieHandler.RequireSsl = settings.RequiresSsl;
        }
    }
}