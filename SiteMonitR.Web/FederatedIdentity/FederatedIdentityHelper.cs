using System;
using System.Text;
using System.Web;
using Microsoft.IdentityModel.Protocols.WSFederation;
using Microsoft.IdentityModel.Web;

namespace SiteMonitR.Web.FederatedIdentity
{
    public static class FederatedIdentityHelper
    {
        public static void LogOn(string issuer = null, string realm = null, string homeRealm = null)
        {
            WSFederationAuthenticationModule fam = FederatedAuthentication.WSFederationAuthenticationModule;
            
            var signInRequest = new SignInRequestMessage(new Uri(issuer ?? fam.Issuer), realm ?? fam.Realm)
            {
                AuthenticationType = fam.AuthenticationType,
                Context = GetReturnUrl(),
                Freshness = fam.Freshness,
                HomeRealm = homeRealm ?? fam.HomeRealm,
                Reply = fam.Reply
            };

            HttpContext.Current.Response.Redirect(signInRequest.WriteQueryString(), false);
            HttpContext.Current.ApplicationInstance.CompleteRequest();
        }


        public static void LogOff()
        {
            WSFederationAuthenticationModule fam = FederatedAuthentication.WSFederationAuthenticationModule;
            fam.SignOut(true);

            HttpContext.Current.Response.Redirect("/", false);
            HttpContext.Current.ApplicationInstance.CompleteRequest();
        }

        public static void FederatedLogOff(string identityProviderSignoutUrl, string replyUrl = null)
        {
            WSFederationAuthenticationModule.FederatedSignOut(new Uri(identityProviderSignoutUrl), new Uri(replyUrl ?? GetReturnUrl()));
        }

        private static string GetReturnUrl()
        {
            var request = HttpContext.Current.Request;
            var reqUrl = request.Url;
            var returnUrl = new StringBuilder();

            returnUrl.Append(reqUrl.Scheme);
            returnUrl.Append("://");
            returnUrl.Append(request.Headers["Host"] ?? reqUrl.Authority);
            returnUrl.Append(request.RawUrl);

            if (!request.ApplicationPath.EndsWith("/", StringComparison.OrdinalIgnoreCase))
            {
                returnUrl.Append("/");
            }

            return returnUrl.ToString();
        }
    }
}