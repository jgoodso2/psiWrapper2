using Microsoft.Office.Project.PWA;
using PwaPSIWrapper.UserCode.PwaGatewayCommands.Entity;
using System;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace PwaPSIWrapper.UserCode.PwaGatewayCommands
{
    public class PwaPublishResourcePlanCommand : IPwaCommand, IPwaCommandFactory, IPwaOutput
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
                return "PwaPublishResourcePlanCommand - publishes a resource plan";
            }
        }

        public string PwaCommandName
        {
            get
            {
                return "PwaPublishResourcePlanCommand";
            }
        }

        public PwaResourcePlanInput PwaInput;
        private PJContext _pj;

        public void Execute()
        {
            try
            {
                var controller = new ResourcePlanController();
                controller.PJContext = _pj.PSI;
                OutputResult = PublishResourcePlan(PwaInput.ResourcePlans, controller, PwaInput.ProjectUID, PwaInput.ResUID,
                    PwaInput.ProjectName, PwaInput.Timescale, PwaInput.Workscale,
                PwaInput.StartDate, PwaInput.EndDate);
            }
            catch (Exception ex)
            {
                OutputResult = new UpdateResult();
                OutputResult.project.projName = PwaInput.ProjectName;
                OutputResult.debugError = ex.Message;
                OutputResult.error = ex.Message;
                OutputResult.success = false;
            }
        }

        public IPwaCommand MakePwaCommand(PJContext pj, NameValueCollection pwaInput)
        {
            return new PwaPublishResourcePlanCommand() { _pj = pj, PwaInput = (PwaResourcePlanInput)new PwaResourcePlanInput(pwaInput).ParseInput() };
        }

        public void ProcessResult(HttpContext context)
        {
            Output = new JavaScriptSerializer().Serialize(OutputResult);
        }

        private UpdateResult PublishResourcePlan(string resourcePlan, ResourcePlanController controller, string puid, string user, string ruid, string timeScale,
            string workScale, string startDate, string endDate)
        {
            UpdateResult result = new UpdateResult();
            DataTable dt = (DataTable)Newtonsoft.Json.JsonConvert.DeserializeObject(resourcePlan, (typeof(DataTable)));
            var rows = dt.AsEnumerable().First(t => t.Field<string>("PROJ_UID") == puid);
            result.project.projName = rows["ProjectName"].ToString();
            return controller.PublishResourcePlan(rows, user, ruid, timeScale, workScale, startDate, endDate);
        }
    }
}



