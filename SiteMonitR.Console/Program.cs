using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiteMonitR.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            WorkerRole.WorkerRole role = new WorkerRole.WorkerRole();
            role.OnStart();
            role.Run();
        }
    }
}
