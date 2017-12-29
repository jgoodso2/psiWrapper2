using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PwaPSIWrapper.UserCode.PwaGatewayCommands.Entity.Pwa
{
    public class Intervals
    {
        public string intervalName {get;set;}
        public string intervalValue { get; set; }
        public string start { get; internal set; }
        public string end { get; internal set; }
    }
}
