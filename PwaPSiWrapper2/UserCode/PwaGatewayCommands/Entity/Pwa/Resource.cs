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
    }
}
