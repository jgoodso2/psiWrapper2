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
using System.Threading.Tasks;
using System.Web.Services.Protocols;
using Microsoft.Office.Project.Server.Library;

namespace PwaPSIWrapper.UserCode.PwaGatewayCommands
{
    public class PwaDeleteResourcePlanCommand : IPwaCommand, IPwaCommandFactory, IPwaOutput
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
                return "PwaDeleteResourcePlanCommand - deletes a resource plan";
            }
        }

        public string PwaCommandName
        {
            get
            {
                return "PwaDeleteResourcePlanCommand";
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
                
                ResPlan[] rp = (ResPlan[])Newtonsoft.Json.JsonConvert.DeserializeObject(PwaInput.ResourcePlans, (typeof(ResPlan[])));
                UpdateResPlan[] resPlans = ResPlan.GetUpdateResPlans(rp);
                UpdateResult[] results = new UpdateResult[resPlans.Count()];
           

                Parallel.ForEach(resPlans, (resPlan, ps, index) =>
                {
                    UpdateResult result = new UpdateResult();
                    try
                    {
                        result = controller.DeleteResourcePlan(resPlan, PwaInput.Timescale, PwaInput.Workscale, PwaInput.StartDate, PwaInput.EndDate);
                    }
                    catch (Exception ex)
                    {
                        Utility.ExceptionUtility.HandleException(ex, resPlan.Project.projName, resPlan.Project.projName, out result);
                    }
                    results[index++] = result;
                });
                OutputResult = results;
            }
            catch (Exception ex)
            {
                OutputResult = new UpdateResult[] { new UpdateResult() { debugError = ex.Message, error = "An unexpected error occured in Save" } };
            }
        }

        public IPwaCommand MakePwaCommand(PJContext pj, NameValueCollection pwaInput)
        {
            return new PwaDeleteResourcePlanCommand() { _pj = pj, PwaInput = (PwaResourcePlanInput)new PwaResourcePlanInput(pwaInput).ParseInput() };
        }

        public void ProcessResult(HttpContext context)
        {
            Output = Newtonsoft.Json.JsonConvert.SerializeObject(OutputResult, OutputResult.GetType(), new Newtonsoft.Json.JsonSerializerSettings());
        }

        private ResPlan[] DeleteResourcePlan(string json,ResourcePlanController controller, string projectUID, string resourceUID, string projectName
           , string timeScale, string workScale, string startDate, string endDate)
        {
            
            ResPlan[] rp = (ResPlan[])Newtonsoft.Json.JsonConvert.DeserializeObject(json, (typeof(ResPlan[])));
            UpdateResPlan[] resPlans = ResPlan.GetUpdateResPlans(rp);
            foreach (var resPlan in resPlans)
            {
                if (controller.DeleteResourcePlan(resPlan, timeScale, workScale, startDate, endDate).success == false)
                {
                    for (var i = 0; i < rp.Count(); i++)
                    {
                        rp[i].projects = rp[i].projects.Where(p => p.projUid.ToUpper() == resPlan.Project.projUid.ToUpper()).ToArray();
                    }
                }
            }
            return rp;
        }
    }
}


