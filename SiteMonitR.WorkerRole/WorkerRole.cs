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
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using SiteMonitR.WorkerRole.Properties;

namespace SiteMonitR.WorkerRole
{
    public class WorkerRole : RoleEntryPoint
    {
        Thread thread;
        Server server;

        public override void Run()
        {
            //Trace.WriteLine("SiteMonitR.WorkerRole entry point called", "Information");

            while (true)
            {
                Thread.Sleep(Settings.Default.PingTimeout);
                server.Run();
            }
        }

        public override bool OnStart()
        {
            ServicePointManager.DefaultConnectionLimit = 12;

            // create the server
            server = new Server(
                new TableStorageSiteUrlRepository(), 
                new WorkerRoleHubConfiguration()
                );

            // run the server
            thread = new Thread(new ThreadStart(() => server.Run()));
            thread.Start();

            return base.OnStart();
        }

        public override void OnStop()
        {
            thread.Abort();
            server.Stop();
        }
    }
}
