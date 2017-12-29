using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PwaPSIWrapper.UserCode.Misc;

namespace PwaPSIWrapper.UserCode.PwaGatewayCommands.Entity
{
    public class PwaGetResourcesInput :IPwaCommandInput
    {
        public PwaGetResourcesInput(NameValueCollection input)
        {
            Input = input;
        }
        

        public NameValueCollection Input
        {
            get; set;
        }

        public IPwaCommandInput ParseInput()
        {
            return this;
        }
    }
}
