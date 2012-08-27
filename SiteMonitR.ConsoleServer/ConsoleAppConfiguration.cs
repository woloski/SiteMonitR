﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace SiteMonitR.ConsoleServer
{
    public class ConsoleAppConfiguration : IHubConfiguration
    {
        public string GetHubContainingSiteUrl()
        {
            return ConfigurationManager.AppSettings["GUI_URL"];
        }
    }
}
