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
        public DateTime From { get; private set; }
        public DateTime To { get; private set; }

        public IPwaCommandInput ParseInput()
        {
            this.ResUID = Input["resuid"];
            this.From = Convert.ToDateTime(Input["start"]);
            this.To = Convert.ToDateTime(Input["end"]);
            return this;
        }
    }
}