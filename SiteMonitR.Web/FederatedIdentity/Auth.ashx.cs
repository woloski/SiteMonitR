using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using Microsoft.IdentityModel.Claims;
using Microsoft.IdentityModel.Protocols.WSFederation;
using Microsoft.IdentityModel.Protocols.WSTrust;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Selectors;
using System.Text;
using System.IdentityModel.Tokens;
using System.IO;
using Microsoft.IdentityModel.Web;
using System.Web.Security;
using System.Collections.ObjectModel;


namespace SiteMonitR.Web.FederatedIdentity
{
    public class Auth : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            var config = new FederatedIdentityConfiguration();
            var tokenXml = GetTokenXml(context.Request);
            var handlers = CreateSecurityTokenHandlerCollection(config.Realm, config.CertificateThumbprint);
            var token = handlers.ReadToken(XmlReader.Create(new StringReader(tokenXml)));
            var identities = handlers.ValidateToken(token);
            var claims = ClaimsPrincipal.CreateFromIdentities(identities);

            IClaimsIdentity identity = claims.Identities[0];

            FederatedAuthentication.SessionAuthenticationModule.WriteSessionTokenToCookie(new SessionSecurityToken(claims, null, token.ValidFrom, token.ValidTo));

            string redirect = "~/";
            if (context.Request["wctx"] != null)
                redirect = context.Request["wctx"];

            context.Response.Redirect(redirect);
            context.ApplicationInstance.CompleteRequest();
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        private static SecurityTokenHandlerCollection CreateSecurityTokenHandlerCollection(string realm, string thumbprint)
        {
            var config = new SecurityTokenHandlerConfiguration();
            config.AudienceRestriction.AllowedAudienceUris.Add(new Uri(realm));
            config.CertificateValidator = X509CertificateValidator.None;
            config.IssuerNameRegistry = new CustomIssuerNameRegistry(thumbprint);
            var handlers = SecurityTokenHandlerCollection.CreateDefaultSecurityTokenHandlerCollection(config);
            handlers.AddOrReplace(new MachineKeySessionSecurityTokenHandler());
            return handlers;
        }

        private static string GetTokenXml(HttpRequest request)
        {
            var quotas = new XmlDictionaryReaderQuotas();
            quotas.MaxArrayLength = 0x200000;
            quotas.MaxStringContentLength = 0x200000;

            var wsFederationMessage = WSFederationMessage.CreateFromFormPost(request) as SignInResponseMessage;
            WSFederationSerializer federationSerializer;
            using (var reader = XmlDictionaryReader.CreateTextReader(Encoding.UTF8.GetBytes(wsFederationMessage.Result), quotas))
            {
                federationSerializer = new WSFederationSerializer(reader);
            }

            var serializationContext = new WSTrustSerializationContext(SecurityTokenHandlerCollectionManager.CreateDefaultSecurityTokenHandlerCollectionManager());
            var tokenXml = federationSerializer.CreateResponse(wsFederationMessage, serializationContext).RequestedSecurityToken.SecurityTokenXml.OuterXml;
            return tokenXml;
        }

        private class CustomIssuerNameRegistry : IssuerNameRegistry
        {
            private readonly List<string> trustedThumbrpints = new List<string>();

            public CustomIssuerNameRegistry(string trustedThumbprint)
            {
                this.trustedThumbrpints.Add(trustedThumbprint);
            }

            public void AddTrustedIssuer(string thumbprint)
            {
                this.trustedThumbrpints.Add(thumbprint);
            }

            public override string GetIssuerName(System.IdentityModel.Tokens.SecurityToken securityToken)
            {
                var x509 = securityToken as X509SecurityToken;
                if (x509 != null)
                {
                    foreach (string thumbprint in trustedThumbrpints)
                    {
                        if (x509.Certificate.Thumbprint.Equals(thumbprint, StringComparison.OrdinalIgnoreCase))
                        {
                            return x509.Certificate.Subject;
                        }
                    }
                }

                return null;
            }
        }

        private class MachineKeySessionSecurityTokenHandler : SessionSecurityTokenHandler
        {
            public MachineKeySessionSecurityTokenHandler()
                : base(CreateTransforms())
            { }

            public MachineKeySessionSecurityTokenHandler(SecurityTokenCache cache, TimeSpan tokenLifetime)
                : base(CreateTransforms(), cache, tokenLifetime)
            { }

            private static ReadOnlyCollection<CookieTransform> CreateTransforms()
            {
                return new List<CookieTransform>
                {
                    new DeflateCookieTransform(),
                    new MachineKeyCookieTransform()
                }.AsReadOnly();
            }

            private class MachineKeyCookieTransform : CookieTransform
            {
                public override byte[] Decode(byte[] encoded)
                {
                    return MachineKey.Decode(Encoding.UTF8.GetString(encoded), MachineKeyProtection.All);
                }

                public override byte[] Encode(byte[] value)
                {
                    return Encoding.UTF8.GetBytes(MachineKey.Encode(value, MachineKeyProtection.All));
                }
            }
        }
    }
}