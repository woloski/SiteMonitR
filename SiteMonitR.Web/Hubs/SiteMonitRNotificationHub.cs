// ---------------------------------------------------------------------------------- 
// Microsoft Developer & Platform Evangelism 
//  
// Copyright (c) Microsoft Corporation. All rights reserved. 
//  
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND,  
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES  
// OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE. 
// ---------------------------------------------------------------------------------- 
// The example companies, organizations, products, domain names, 
// e-mail addresses, logos, people, places, and events depicted 
// herein are fictitious.  No association with any real company, 
// organization, product, domain name, email address, logo, person, 
// places, or events is intended or should be inferred. 
// ---------------------------------------------------------------------------------- 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SignalR.Hubs;
using System.Threading;
using SiteMonitR.Web.FederatedIdentity;

namespace SiteMonitR.Web.Hubs
{
    [HubName("SiteMonitR")]
    public class SiteMonitRNotificationHub : Hub
    {
        FederatedIdentityConfiguration authConfig;

        public SiteMonitRNotificationHub(FederatedIdentityConfiguration authConfig)
        {
            this.authConfig = authConfig;
        }

        public SiteMonitRNotificationHub()
            : this(new FederatedIdentityConfiguration())
        {
        }

        public void IsAuthenticated()
        {
            if (this.authConfig.AuthEnabled)
            {
                Caller.isAuthenticated(new 
                { 
                    isAuthenticated = Thread.CurrentPrincipal.Identity.IsAuthenticated, 
                    realm = this.authConfig.Realm, 
                    serviceNamespace = this.authConfig.ServiceNamespace 
                });
                
                return;
            }
            else
            {
                Caller.isAuthenticated(new { isAuthenticated = true });
            }
        }

        public void ServiceReady()
        {
            Clients.serviceIsUp();
        }

        public void ReceiveMonitorUpdate(dynamic monitorUpdate)
        {
            Clients.siteStatusUpdated(monitorUpdate);
        }

        public void AddSiteToGui(string url)
        {
            Clients.siteAddedToGui(url);
        }

        public void RemoveSiteFromGui(string url)
        {
            Clients.siteRemovedFromGui(url);
        }

        public void AddSite(string url, string test)
        {
            this.ThrowIfNotAuthenticated();
            Clients.siteAddedToStorage(url, test);
        }

        public void RemoveSite(string url)
        {
            this.ThrowIfNotAuthenticated();
            Clients.siteRemovedFromStorage(url);
        }

        public void GetSiteList()
        {
            this.ThrowIfNotAuthenticated();
            Clients.siteListRequested();
        }

        public void ListOfSitesObtained(List<Site> sites)
        {
            Clients.siteListObtained(sites);
        }

        public void CheckSite(string url)
        {
            Clients.checkingSite(url);
        }

        private void ThrowIfNotAuthenticated()
        {
            if (this.authConfig.AuthEnabled)
            {
                if (!Thread.CurrentPrincipal.Identity.IsAuthenticated)
                    throw new UnauthorizedAccessException();
            }
        }
    }
}
