using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PwaPSIWrapper.UserCode.PwaGatewayCommands.Entity.Pwa
{
    public class UpdateResource
    {
        public Resource resource { get; set; }
        public Intervals[] intervals { get; set; }

        public string this[string intervalName]
        {
            get
            {
                return intervals.First(t => t.intervalName == intervalName).intervalValue;
            }
        }
    }
}
