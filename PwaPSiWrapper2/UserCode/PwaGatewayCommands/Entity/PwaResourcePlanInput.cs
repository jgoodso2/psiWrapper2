using System;
using System.Collections.Specialized;
using Microsoft.Office.Project.Server.Schema;

namespace PwaPSIWrapper.UserCode.PwaGatewayCommands
{
    public class PwaResourcePlanInput : IPwaCommandInput
    {
        

        public PwaResourcePlanInput(NameValueCollection pwaInput)
        {
            this.Input = pwaInput;
        }

        public string CheckedOutBy { get; private set; }
        public string EndDate { get; private set; }
        public NameValueCollection Input
        {
            get;
            set;
        }

        public string isCheckedOut { get; set; }
        public string ProjectName { get; private set; }
        public string ProjectUID { get; private set; }
        public string ResourcePlans { get; internal set; }
        public string ResUID { get; private set; }
        public string StartDate { get; private set; }
        public string Timescale { get; private set; }
        public string User { get; internal set; }
        public string Workscale { get; private set; }
        public string ViewGuid { get; private set; }

        public IPwaCommandInput ParseInput()
        {
            this.ProjectUID = Input["puid"];
            this.ResUID = Input["resuid"];
            this.ProjectName = Input["projname"];
            this.Timescale = Input["timeScale"];
            this.Workscale = Input["workScale"];
            this.StartDate = Input["fromDate"];
            this.EndDate = Input["toDate"];
            this.ViewGuid = Input["viewguid"];
            this.ResourcePlans = Input["resourceplan"];
            this.isCheckedOut = Input["ischeckedout"];
            this.CheckedOutBy = Input["checkoutby"];
            return this;
        }
    }
}