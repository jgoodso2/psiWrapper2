using Microsoft.Office.Project.PWA;
using PwaPSIWrapper;
using PwaPSIWrapper.UserCode.PwaGatewayCommands;
using PwaPSIWrapper.UserCode.PwaGatewayCommands.Entity;
using PwaPSIWrapper.UserCode.PwaGatewayCommands.Entity.Pwa;
using PwaPSiWrapper2.UserCode.PwaGatewayCommands.Entity.Pwa;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace PwaPSiWrapper2.UserCode.PwaGatewayCommands
{
    public class PwaGetTimsheetsCommand : IPwaCommand, IPwaCommandFactory, IPwaOutput
    {
        public string Output
        {
            get;
            set;
        }

        public Dictionary<string, TimesheetCapacityData> OutputResult
        {
            get;
            set;
        }
        public string PwaCommandDescription
        {
            get
            {
                return "PwaGetTimsheetsCommand - gets timesheet data";
            }
        }

        public string PwaCommandName
        {
            get
            {
                return "PwaGetTimsheetsCommand";
            }
        }

        public PwaGetTimesheetsInput PwaInput;
        private PJContext _pj;
        UpdateResult result = null;

        public void Execute()
        {
            try
            {
                var controller = new ResourcePlanController();
                controller.PJContext = _pj.PSI;
                OutputResult = GetTimesheets(_pj.PSI, PwaInput.ResUID,PwaInput.From,PwaInput.To);
            }
            catch (Exception ex)
            {
                result = new UpdateResult();
                result.success = false;
                result.debugError = ex.Message;
                result.error = ex.Message;
            }
        }

        public IPwaCommand MakePwaCommand(PJContext pj, NameValueCollection pwaInput)
        {
            //inconsequential change
            return new PwaGetTimsheetsCommand() { _pj = pj, PwaInput = (PwaGetTimesheetsInput)new PwaGetTimesheetsInput(pwaInput).ParseInput() };
        }

        public void ProcessResult(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            Output = Newtonsoft.Json.JsonConvert.SerializeObject(OutputResult);
        }

        private Dictionary<string, TimesheetCapacityData> GetTimesheets(Microsoft.Office.Project.PWA.PSI psi, string resuid,DateTime fromDate,DateTime toDate)
        {
            try
            {
                var controller = new ResourcePlanController() { PJContext = psi };
                return controller.GetTimesheet(resuid,fromDate,toDate);
            }
            catch (Exception ex)
            {
                //if (ex is SoapException)
                //{
                //    var error = new Microsoft.Office.Project.Server.Library.PSClientError(ex as SoapException);
                //    var errors = error.GetAllErrors();
                //    if (errors.Any(t => t.ErrId == Microsoft.Office.Project.Server.Library.PSErrorID.GeneralSecurityAccessDenied))
                //    {
                //        return new ResPlan[] { new ResPlan() { resource = new Resource() { resUid=Guid.Empty.ToString()}
                //        ,projects=new Project[] { new Project(){ projUid=puid,projName=projName,intervals=new Intervals[] { } ,readOnly = true,
                //            readOnlyReason ="You are not authorized to edit the resource plan for the project " + projName} }
                //        }
                //        };
                //    }
                //}
                return new Dictionary<string, TimesheetCapacityData>();
            }
        }
    }
}
