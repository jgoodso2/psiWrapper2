using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Office.Project.PWA;
using PwaPSIWrapper.UserCode.PwaGatewayCommands.Entity;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Web.Script.Serialization;
using PJSchema = Microsoft.Office.Project.Server.Schema;
using System.Data;
using PSLib = Microsoft.Office.Project.Server.Library;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Configuration;
using PwaPSIWrapper.UserCode.PwaGatewayCommands.Entity.Pwa;
using PwaPSIWrapper.UserCode.Utility;

namespace PwaPSIWrapper.UserCode.PwaGatewayCommands
{
    public class PwaAddResourcesToPlanCommand : IPwaCommand, IPwaCommandFactory, IPwaOutput
    {
        public string Output
        {
            get;
            set;
        }

        public UpdateResult OutputResult
        {
            get;
            set;
        }
        public string PwaCommandDescription
        {
            get
            {
                return "PwaAddResourcesToPlanCommand - add resources to a resource plan";
            }
        }

        public string PwaCommandName
        {
            get
            {
                return "PwaAddResourcesToPlanCommand";
            }
        }

        public PwaAddResourcesInput PwaInput;
        private PJContext _pj;

        public void Execute()
        {
            try
            {
                var controller = new ResourcePlanController();
                controller.PJContext = _pj.PSI;
                OutputResult = AddResourcePlan(controller,PwaInput.ProjectUID, PwaInput.ResUIDs,
                        PwaInput.ProjectName, PwaInput.Timescale, PwaInput.Workscale,
                PwaInput.StartDate, PwaInput.EndDate);
            }
            catch (Exception ex)
            {
                OutputResult = new UpdateResult();
                OutputResult.project.projName = PwaInput.ProjectName;
                OutputResult.project.projUid = PwaInput.ProjectUID;
                OutputResult.error = ex.Message;
                OutputResult.debugError = ex.Message;
                OutputResult.success = false;
            }
        }

        public IPwaCommand MakePwaCommand(PJContext pj, NameValueCollection pwaInput)
        {
            return new PwaAddResourcesToPlanCommand() { _pj = pj, PwaInput = (PwaAddResourcesInput)new PwaAddResourcesInput(pwaInput).ParseInput() };
        }

        public void ProcessResult(HttpContext context)
        {
            Output = new JavaScriptSerializer().Serialize(OutputResult);
        }

        private UpdateResult AddResourcePlan(ResourcePlanController controller, string projectUID, string[] resourceUIDs, string projectName
           , string timeScale, string workScale, string startDate, string endDate)
        {
            return  controller.AddResourcePlan(projectUID, projectName, resourceUIDs, timeScale, workScale, startDate, endDate);
            
        }


    }
}
