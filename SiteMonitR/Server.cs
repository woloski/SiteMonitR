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
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Timers;
using SignalR.Client.Hubs;
using System.Dynamic;
using System.IO;

namespace SiteMonitR
{
    public class Server
    {
        Timer _timer;
        HubConnection _connection;
        IHubProxy _hub;
        ISiteUrlRepository _siteRepository;
        IHubConfiguration _hubConfiguration;

        public Server(ISiteUrlRepository siteRepository,
            IHubConfiguration hubConfiguration)
        {
            _siteRepository = siteRepository;
            _hubConfiguration = hubConfiguration;
        }

        private void Log(string log)
        {
            Console.WriteLine(log, "Information");
            Trace.WriteLine(log, "Information");
        }

        public void Stop()
        {
            if (_timer != null)
                _timer.Stop();
        }

        public void Run()
        {
            if (_timer != null && _timer.Enabled)
                return;

            var url = _hubConfiguration.GetHubContainingSiteUrl();

            try
            {
                if (_connection == null)
                {
                    // connect to the hub
                    _connection = new HubConnection(url);

                    // create a proxy
                    _hub = _connection.CreateProxy("SiteMonitR");

                    // whenever a site is added
                    _hub.On<string, string>("siteAddedToStorage", (siteUrl, test) =>
                        {
                            _siteRepository.Add(new Site { Url = siteUrl, Test = test });
                            _hub.Invoke("addSiteToGui", siteUrl);
                        });

                    // whenever a site is removed
                    _hub.On<string>("siteRemovedFromStorage", (siteUrl) =>
                        {
                            _siteRepository.Remove(siteUrl);
                            _hub.Invoke("removeSiteFromGui", siteUrl);
                        });

                    // whenever the list of sites is requested
                    _hub.On("siteListRequested", () =>
                        {
                            var sites = _siteRepository.GetUrls();
                            _hub.Invoke("listOfSitesObtained", sites);
                        });

                    // now start the connection
                    _connection.Start().ContinueWith((t) =>
                        {
                            _hub.Invoke("serviceReady");
                        });
                }

                _timer = new Timer(_hubConfiguration.GetPingTimeout());

                _timer.Elapsed += (s, e) =>
                {
                    _timer.Stop();

                    _siteRepository.GetUrls()
                       .ForEach(site =>
                       {
                           dynamic result = new ExpandoObject();
                           result.ping = false;

                           try
                           {
                               // inform the client the site is being checked
                               _hub.Invoke("checkSite", site.Url);

                               // check the site
                               var output = new WebClient().DownloadString(site.Url);

                               if (!string.IsNullOrEmpty(site.Test))
                               {
                                   string tempFile = Path.GetTempFileName() + ".js";
                                   File.WriteAllText(tempFile, site.Test);

                                   string phantom = Executor.Execute("phantomjs.exe", tempFile + " " + site.Url);

                                   File.Delete(tempFile);
                                   //Log("phantomjs test: " + site.Test);
                                   Log("phantomjs result: " + phantom);
                                   result.testResult = phantom;
                               }

                               result.ping = true;
                               Log(site.Url + " is up");
                           }
                           catch
                           {
                               result.ping = false;
                               Log(site.Url + " is down");
                           }

                           // invoke a method on the hub
                           _hub.Invoke("receiveMonitorUpdate", new
                           {
                               Url = site.Url,
                               Result = result
                           });
                       });

                    _timer.Start();

                    Log("All sites pinged. Sleeping...");
                };

                _timer.Start();
            }
            catch
            {
                if (_timer != null)
                    _timer.Stop();
            }
        }
    }

    public class Executor
    {
        public static string Execute(string program, string arguments)
        {
            StringBuilder outputBuilder;
            ProcessStartInfo processStartInfo;
            Process process;

            outputBuilder = new StringBuilder();

            processStartInfo = new ProcessStartInfo();
            processStartInfo.CreateNoWindow = true;
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.RedirectStandardInput = true;
            processStartInfo.UseShellExecute = false;
            processStartInfo.Arguments = arguments;
            processStartInfo.FileName = program;

            process = new Process();
            process.StartInfo = processStartInfo;
            // enable raising events because Process does not raise events by default
            process.EnableRaisingEvents = true;
            // attach the event handler for OutputDataReceived before starting the process
            process.OutputDataReceived += new DataReceivedEventHandler
            (
                delegate(object sender, DataReceivedEventArgs e)
                {
                    // append the new data to the data already read-in
                    outputBuilder.Append(e.Data);
                    outputBuilder.Append(Environment.NewLine);
                }
            );
            // start the process
            // then begin asynchronously reading the output
            // then wait for the process to exit
            // then cancel asynchronously reading the output
            process.Start();
            process.BeginOutputReadLine();
            process.WaitForExit();
            process.CancelOutputRead();

            // use the output
            string output = outputBuilder.ToString();

            return output;
        }
    }
}
