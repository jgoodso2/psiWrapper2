using Microsoft.Office.Project.PWA;
using PwaPSIWrapper.UserCode.PwaGatewayCommands.Entity;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Web.Script.Serialization;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.SessionState;
using PJSchema = Microsoft.Office.Project.Server.Schema;
using PSPJLib = Microsoft.Office.Project.PWA;
using PwaPSIWrapper.UserCode.PwaGatewayCommands.Entity.Pwa;
using System.Web.Services.Protocols;

namespace PwaPSIWrapper.UserCode.PwaGatewayCommands
{
    public class PwaGetProjectPlansCommand : IPwaCommand, IPwaCommandFactory, IPwaOutput
    {
        public string Output
        {
            get;
            set;
        }

        public ProjectPlan OutputResult
        {
            get;
            set;
        }
        public string PwaCommandDescription
        {
            get
            {
                return "PwaGetProjectPlansCommand - gets resource plans";
            }
        }

        public string PwaCommandName
        {
            get
            {
                return "PwaGetProjectPlansCommand";
            }
        }

        public PwaProjectPlanInput PwaInput;
        private PJContext _pj;
        UpdateResult result = null;

        public void Execute()
        {
            try
            {
                var controller = new ResourcePlanController();
                controller.PJContext = _pj.PSI;
                OutputResult = GetProjectPlans(_pj.PSI,PwaInput.ProjectUID, PwaInput.ProjectName, Convert.ToDateTime(PwaInput.StartDate), Convert.ToDateTime(PwaInput.EndDate),
                    PwaInput.Timescale, PwaInput.Workscale, false, PwaInput.isCheckedOut, PwaInput.CheckedOutBy);
            }
            catch(Exception ex)
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
            return new PwaGetProjectPlansCommand() { _pj = pj, PwaInput = (PwaProjectPlanInput)new PwaProjectPlanInput(pwaInput).ParseInput() };
        }

        public void ProcessResult(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            context.Response.Cookies.Add(new HttpCookie("ResPlanByRes", context.Request.Form["resuid"]));
            context.Response.Cookies.Add(new HttpCookie("ResPlanByResName", context.Request.Form["resname"]));
            context.Response.Cookies.Add(new HttpCookie("ResPlanByResStart", context.Request.Form["fromDate"]));
            context.Response.Cookies.Add(new HttpCookie("ResPlanByResEnd", context.Request.Form["toDate"]));
            context.Response.Cookies.Add(new HttpCookie("ResPlanByResTimeScale", context.Request.Form["timeScale"]));
            context.Response.Cookies.Add(new HttpCookie("ResPlanByResWorkScale", context.Request.Form["workScale"]));
            if (result != null)
            {
                Output = new JavaScriptSerializer().Serialize(result);
            }
            else
            {
                Output = new JavaScriptSerializer().Serialize(OutputResult);
            }
        }

        private ProjectPlan GetProjectPlans(Microsoft.Office.Project.PWA.PSI psi, string puid, string projName, DateTime startDate, DateTime endDate, string timeScale,
           string workScale, bool isPSIVersion, string isCheckedOut, string checkOutBY)
        {
            try
            {
                var controller = new ResourcePlanController() { PJContext = psi };
                return controller.GetProjectPlan(puid, projName, startDate, endDate, timeScale, workScale, isPSIVersion, isCheckedOut, checkOutBY);
            }
            catch (Exception ex)
            {
                if (ex is SoapException)
                {
                    var error = new Microsoft.Office.Project.Server.Library.PSClientError(ex as SoapException);
                    var errors = error.GetAllErrors();
                    if (errors.Any(t => t.ErrId == Microsoft.Office.Project.Server.Library.PSErrorID.GeneralSecurityAccessDenied))
                    {
                        return  new ProjectPlan() { resources = new []{new Resource() { resUid=Guid.Empty.ToString() } }
                        ,project= new Project(){ projUid=puid,projName=projName,intervals=new Intervals[] { } ,readOnly = true,
                            readOnlyReason ="You are not authorized to edit the resource plan for the project " + projName} 
                        };
                    }
                }
                return new ProjectPlan() { resources = new[] { new Resource() { resUid = Guid.Empty.ToString() } } } ;

            }

        }

        private List<string> GetColumnsFromResourcePlanDataSet(PJSchema.ResourcePlanDataSet resourcePlan, string timeScale)
        {
            List<string> Columns = new List<string>();
            Columns.Add("ProjectName");
            Columns.Add("PROJ_UID");
            foreach (PJSchema.ResourcePlanDataSet.DatesRow interval in resourcePlan.Dates)
            {
                if (timeScale == "Calendar Months" || timeScale == "Weeks")
                {
                    Columns.Add(interval.StartDate.ToShortDateString() + " - " + interval.EndDate.AddDays(-1).ToShortDateString());
                }
                else
                {
                    Columns.Add(interval.StartDate.ToShortDateString() + " - " + interval.EndDate.ToShortDateString());
                }
            }

            return Columns;
        }

        private List<ColumnModel> GetColumnsModelFromResourcePlanDataSet(PJSchema.ResourcePlanDataSet resourcePlan)
        {
            List<ColumnModel> Columns = new List<ColumnModel>();
            ColumnModel projectNameColumn = new ColumnModel() { name = "ProjectName", index = "ProjectName", width = "200", align = "left", editable = false, sortable = true };
            ColumnModel projectIDColumn = new ColumnModel() { name = "PROJ_UID", index = "PROJ_UID", width = "200", align = "center", editable = false, sortable = true, hidden = true };
            Columns.Add(projectNameColumn);
            Columns.Add(projectIDColumn);
            foreach (PJSchema.ResourcePlanDataSet.DatesRow interval in resourcePlan.Dates)
            {

                ColumnModel intervalColumn = new ColumnModel() { name = interval.IntervalName, index = interval.IntervalName, width = "75", align = "center", editable = true, formatter = "textBoxFormatter" };
                Columns.Add(intervalColumn);


            }
            return Columns;
        }
        private List<Dictionary<string, Object>> GetJsonFromDataTable(DataTable dt)
        {
            System.Web.Script.Serialization.JavaScriptSerializer serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();
            Dictionary<string, object> row;
            foreach (DataRow dr in dt.Rows)
            {
                row = new Dictionary<string, object>();
                foreach (DataColumn col in dt.Columns)
                {
                    if (col.DataType == typeof(Single))
                    {
                        if (dr[col] == null || dr[col] == System.DBNull.Value)
                        {
                            row.Add(col.ColumnName, Convert.ToSingle(0).ToString("N2"));
                        }
                        else
                        {
                            row.Add(col.ColumnName, Convert.ToSingle(dr[col]).ToString("N2"));
                        }
                    }
                    else
                    {
                        row.Add(col.ColumnName, dr[col]);
                    }
                }
                rows.Add(row);
            }
            return rows;
        }
    }
   
}



