
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Office.Project.PWA;
using PwaPSIWrapper.UserCode.PwaGatewayCommands.Entity;
using System.Collections.Specialized;
using PwaPSIWrapper.UserCode.PwaGatewayCommands;
using PSLib = Microsoft.Office.Project.Server.Library;
using System.Web;
using System.Web.Script.Serialization;

namespace PwaPSIWrapper
{
    public class PwaPublishCommand : IPwaCommand, IPwaCommandFactory, IPwaOutput
    {
        PwaCommandProjectInput input;
        PJContext _pj;

        public string PwaCommandDescription
        {
            get { return "PwaPublish command - needs a ProjectUID"; }
        }

        public string PwaCommandName
        {
            get        { return "PwaPublish"; } 
            set { PwaCommandName = value;  } 
        }

        public string Output
        {
            get;
            set;
        }

        public bool OutputResult
        {
            get;
            set;
        }
        public void Execute()
        {
            try {
                var jobGuid = Guid.NewGuid();
                _pj.PSI.ProjectWebService.QueuePublish(jobGuid, input.ProjUID.FirstOrDefault(), true, null);
                OutputResult = QueueHelper.WaitForQueueJobCompletion(jobGuid, (int)PSLib.QueueConstants.QueueMsgType.ResourcePlanPublish, _pj.PSI);
            }
            catch(Exception ex)
            {
                OutputResult = false;
            }
        }

        public void ProcessResult(HttpContext context)
        {
            Output = new JavaScriptSerializer().Serialize(OutputResult);
        }

        public IPwaCommand MakePwaCommand(PJContext pj, NameValueCollection args)
        {
            
            return new PwaPublishCommand { _pj = pj, input = (PwaCommandProjectInput)new PwaCommandProjectInput(args).ParseInput() };
        }


        #region scrap
        

        #endregion


    }
}
