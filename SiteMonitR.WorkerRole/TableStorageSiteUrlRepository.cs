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

using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.StorageClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace SiteMonitR.WorkerRole
{
    public class TableStorageSiteUrlRepository : ISiteUrlRepository
    {
        private string _connectionStringName = "SiteMonitRConnectionString";
        private string _tableName = "sitemonitrurls";
        private CloudStorageAccount _storageAccount;
        private CloudTableClient _tableClient;
        private TableServiceContext _tableContext;

        public TableStorageSiteUrlRepository()
        {
            _storageAccount = CloudStorageAccount.Parse(
                CloudConfigurationManager.GetSetting(_connectionStringName)
                );

            _tableClient = new CloudTableClient(_storageAccount.TableEndpoint.AbsoluteUri, _storageAccount.Credentials);
            _tableClient.RetryPolicy = RetryPolicies.Retry(3, TimeSpan.FromSeconds(1));
            _tableClient.CreateTableIfNotExist(_tableName);
            _tableContext = _tableClient.GetDataServiceContext();
        }

        public List<Site> GetUrls()
        {
            var r = _tableContext.CreateQuery<StoredSiteUrl>(_tableName);
            return r.ToList().Select(x => new Site { Url = x.Url, Test = x.Test }).ToList();
        }

        public void Add(Site site)
        {
            _tableContext.AddObject(_tableName, new StoredSiteUrl { Url = site.Url, Test = site.Test });
            _tableContext.SaveChanges();
        }

        public void Remove(string url)
        {
            var o = _tableContext.CreateQuery<StoredSiteUrl>(_tableName).ToList().First(x => x.Url == url);
            _tableContext.DeleteObject(o);
            _tableContext.SaveChanges();
        }
    }
}
