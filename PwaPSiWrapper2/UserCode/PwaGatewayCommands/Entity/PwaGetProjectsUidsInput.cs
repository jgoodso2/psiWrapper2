using System;
using System.Collections.Specialized;

namespace PwaPSIWrapper.UserCode.PwaGatewayCommands
{
    public class PwaGetProjectsUidsInput : IPwaCommandInput
    {
        public PwaGetProjectsUidsInput(NameValueCollection pwaInput)
        {
            this.Input = pwaInput;
        }

        public NameValueCollection Input
        {
            get;
            set;
        }

        public string Ruid
        {
            get; set;
        }

       

        public IPwaCommandInput ParseInput()
        {
            Ruid = Input["resuid"];
            return this;
        }
    }
}