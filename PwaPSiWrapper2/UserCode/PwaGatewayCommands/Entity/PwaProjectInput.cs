using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PwaPSIWrapper.UserCode.Misc;
using System.Web.Script.Serialization;

namespace PwaPSIWrapper.UserCode.PwaGatewayCommands.Entity
{
    public class PwaCommandProjectInput : IPwaCommandInput
    {
        public Guid[] ProjUID;

        public PwaCommandProjectInput(NameValueCollection args)
        {
            Input = args;
        }

        public NameValueCollection Input
        {
            get;
            set;
        }

        public IPwaCommandInput ParseInput()
        {
            this.ProjUID = new Guid[] { new Guid(Input["ProjectUID"]) };
            return this;
        }
    }
}
