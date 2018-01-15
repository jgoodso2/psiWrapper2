using System;
using System.Collections.Specialized;
using Microsoft.Office.Project.Server.Schema;

namespace PwaPSIWrapper.UserCode.PwaGatewayCommands
{
    public class PwaGetTimesheetsInput : IPwaCommandInput
    {
        

        public PwaGetTimesheetsInput(NameValueCollection pwaInput)
        {
            this.Input = pwaInput;
        }

        public NameValueCollection Input
        {
            get;
            set;
        }

        public string ResUID { get; private set; }
        public string Workscale { get; private set; }

        public IPwaCommandInput ParseInput()
        {
            this.ResUID = Input["resuid"];
            this.Workscale = Input["workScale"];
            return this;
        }
    }
}