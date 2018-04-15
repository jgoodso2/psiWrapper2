using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PwaPSIWrapper.UserCode.PwaGatewayCommands.Entity.Pwa
{
    public class Resource
    {
        public string resUid { get; set; }
        public string resName { get; set; }
        public bool selected { get; set; }
        public CustomField[] CustomFields { get; set; }
        public Intervals[] intervals { get; set; }
        public Intervals[] capacity { get; set; }

        public string this[string intervalName]
        {
            get
            {
                return intervals.First(t => t.intervalName == intervalName).intervalValue;
            }
        }
    }
}
