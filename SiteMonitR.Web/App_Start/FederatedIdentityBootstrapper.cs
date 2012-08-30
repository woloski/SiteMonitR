using System;
using System.IdentityModel.Selectors;
using Microsoft.IdentityModel.Web;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using SiteMonitR.Web.FederatedIdentity.Infrastructure;
using SiteMonitR.Web.FederatedIdentity;
using System.Web;

[assembly: WebActivator.PreApplicationStartMethod(typeof(SiteMonitR.Web.App_Start.FederatedIdentityBootstrapper), "PreAppStart")]

namespace SiteMonitR.Web.App_Start
{
    public static class FederatedIdentityBootstrapper
    {
        public static void PreAppStart()
        {
            DynamicModuleUtility.RegisterModule(typeof(CustomSessionAuthenticationModule));
        }
    }
}