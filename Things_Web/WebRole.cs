using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;

namespace Things_Web
{
    public class WebRole : RoleEntryPoint
    {
        public override bool OnStart()
        {
            CloudStorageAccount.Parse(RoleEnvironment.GetConfigurationSettingValue("DataConnectionString")).CreateCloudBlobClient().GetContainerReference("things").CreateIfNotExist();
            return base.OnStart();
        }
    }
}