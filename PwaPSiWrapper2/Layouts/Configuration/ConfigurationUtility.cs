using Microsoft.SharePoint.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace PwaPSIWrapper.Configuration
{
    public static class ConfigurationUtility
    {
        public static string GetConnectionString(string name)
        {
            var fileName = SPUtility.GetGenericSetupPath("") + "template/Layouts/Configuration/Configuration.Xml";
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(fileName);
            return xmlDoc.SelectSingleNode(string.Format("Configuration/ConnectionStrings/ConnectionString[@Name='{0}']/@ConnectionString", name)).Value;
        }

    }
}
