using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PwaPSIWrapper.UserCode.PwaGatewayCommands.Entity.Pwa
{
    public class Project
    {
        public string projUid { get; set; }
        public string projName { get; set; }

        public bool selected { get; set; }
        public Intervals[] intervals { get; set; }
        public bool readOnly { get; set; }
        public bool stalePublish { get; set; }
        public string readOnlyReason { get; set; }
        public string this[string intervalName]
        {
            get
            {
                return intervals.First(t => t.intervalName == intervalName).intervalValue;
            }
        }

        public CustomField[] CustomFields
        {
            get;set;
        }
    }
}
