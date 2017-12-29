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
using Microsoft.Office.Project.Server.Library;
using System.Web.Services.Protocols;
using System.Threading.Tasks;

namespace PwaPSIWrapper.UserCode.PwaGatewayCommands
{
    public class PwaUpdateResourcePlanCommand : IPwaCommand, IPwaCommandFactory, IPwaOutput
    {
        public string Output
        {
            get;
            set;
        }

        public UpdateResult[] OutputResult
        {
            get;
            set;
        }
        public string PwaCommandDescription
        {
            get
            {
                return "PwaUpdateResourcePlanCommand - updates a resource plan";
            }
        }

        public string PwaCommandName
        {
            get
            {
                return "PwaupdateResourcePlanCommand";
            }
        }

        public PwaResourcePlanInput PwaInput;
        private PJContext _pj;

        public void Execute()
        {
            try {
                var controller = new ResourcePlanController();
                controller.PJContext = _pj.PSI;
                //dt.AsEnumerable().First(t=>t.Field<string>("PROJ_UID") ==  context.Request.Form["projectuid"].ToString()
                //OutputResult = UpdateResourcePlan(PwaInput.ResourcePlans, controller, PwaInput.ProjectUID, "", PwaInput.ResUID, PwaInput.Timescale, PwaInput.Workscale,
                //    PwaInput.StartDate, PwaInput.EndDate);
                OutputResult = UpdateResourcePlanAsync(PwaInput.ResourcePlans, controller, PwaInput.Timescale, PwaInput.Workscale,
                    PwaInput.StartDate, PwaInput.EndDate);
            }
            catch (Exception ex)
            {
                //OutputResult = new UpdateResult();
                
            }
        }


        public IPwaCommand MakePwaCommand(PJContext pj, NameValueCollection pwaInput)
        {
            return new PwaUpdateResourcePlanCommand() { _pj = pj, PwaInput = (PwaResourcePlanInput)new PwaResourcePlanInput(pwaInput).ParseInput() };
        }

        public void ProcessResult(HttpContext context)
        {
            Output = new JavaScriptSerializer().Serialize(OutputResult);
        }
        private UpdateResult[] UpdateResourcePlan(string json, ResourcePlanController controller, string puid, string user, string ruid, string timeScale,
            string workScale, string startDate, string endDate)
        {
            try
            {
                UpdateResult result = new UpdateResult();
                ResPlan[] rp = (ResPlan[])Newtonsoft.Json.JsonConvert.DeserializeObject(json, (typeof(ResPlan[])));
                UpdateResPlan[] resPlans = ResPlan.GetUpdateResPlans(rp);
                UpdateResult[] results = new UpdateResult[resPlans.Count()];
                var counter = 0;
                foreach (var resPlan in resPlans)
                {
                    result =  controller.UpdateResourcePlan(resPlan.Project.resources.ToArray(), resPlan.Project.projUid, timeScale, workScale, startDate, endDate);
                    results[counter++] = result;
                }
                return results;
                
            }
            catch(Exception ex)
            {
                return new UpdateResult[] { new UpdateResult() { debugError = ex.Message, error = "An unexpected error occured in Save", project = new Entity.Pwa.Project() { projUid = puid } } };
            }
            
        }

        private UpdateResult[] UpdateResourcePlanAsync(string json, ResourcePlanController controller, string timeScale,
            string workScale, string startDate, string endDate)
        {
            try
            {
               //get json data
                ResPlan[] rp = (ResPlan[])Newtonsoft.Json.JsonConvert.DeserializeObject(json, (typeof(ResPlan[])));
                //pivot data into project based resplans
                UpdateResPlan[] resPlans = ResPlan.GetUpdateResPlans(rp);
                //create output data
                UpdateResult[] results = new UpdateResult[resPlans.Count()];
                Parallel.ForEach(resPlans, (resPlan, ps, index) =>
                {
                    try
                    {
                        UpdateResult result = new UpdateResult() { project = new Entity.Pwa.Project() { projUid = resPlan.Project.projUid, projName = resPlan.Project.projName } };

                        result = controller.UpdateResourcePlan(resPlan.Project.resources.ToArray(), resPlan.Project.projUid, timeScale, workScale, startDate, endDate);
                        result.project = new Entity.Pwa.Project() { projUid = resPlan.Project.projUid, projName = resPlan.Project.projName };
                        results[index] = result;
                    }
                    catch (Exception ex)
                    {
                        results[index] = new UpdateResult() { debugError = ex.Message, error = "An unexpected error occured in Save", project = new Entity.Pwa.Project() { projUid = resPlan.Project.projUid } } ;
                    }

                });
                return results;
            }
            catch (Exception ex)
            {
                return new UpdateResult[] { new UpdateResult() { debugError = ex.Message, error = "An unexpected error occured in Save",project=new Entity.Pwa.Project(){ } } };
            }

        }
    }
}
