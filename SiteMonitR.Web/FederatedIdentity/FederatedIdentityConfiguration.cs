using System;
using System.Configuration;

namespace SiteMonitR.Web.FederatedIdentity
{
    public class FederatedIdentityConfiguration
    {
        public bool AuthEnabled
        {
            get
            {
                bool enabled = false;
                bool enabledParsed = Boolean.TryParse(ConfigurationManager.AppSettings["fedauth.enabled"], out enabled);
                return enabledParsed && enabled;
            }
        }

        public string Realm
        {
            get
            {
                return ConfigurationManager.AppSettings["fedauth.realm"];
            }
        }

        public string CertificateThumbprint
        {
            get
            {
                return ConfigurationManager.AppSettings["fedauth.certThumbprint"];
            }
        }

        public string ServiceNamespace
        {
            get
            {
                return ConfigurationManager.AppSettings["fedauth.waad.serviceNamespace"];
            }
        }

        public bool RequiresSsl
        {
            get
            {
                bool requireSsl = false;
                bool requireSslParsed = Boolean.TryParse(ConfigurationManager.AppSettings["fedauth.requireSsl"], out requireSsl);
                return requireSslParsed && requireSsl;
            }
        }
    }
}