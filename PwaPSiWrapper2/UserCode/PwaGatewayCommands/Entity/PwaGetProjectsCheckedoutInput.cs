using System.Collections.Specialized;

namespace PwaPSIWrapper.UserCode.PwaGatewayCommands
{
    public class PwaGetProjectsCheckedoutInput : IPwaCommandInput
    {
        public PwaGetProjectsCheckedoutInput(NameValueCollection pwaInput)
        {
            this.Input = pwaInput;
        }

        public NameValueCollection Input
        {
            get;
            set;
        }

        public string SelectedProject
        {
            get; set;
        }


        public IPwaCommandInput ParseInput()
        {
            SelectedProject = Input["selectedProj"];
            return this;
        }
    }
}