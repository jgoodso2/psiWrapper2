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
    public class PwaGetResourcePlansCommand : IPwaCommand, IPwaCommandFactory, IPwaOutput
    {
        public string Output
        {
            get;
            set;
        }

        public ResPlan[] OutputResult
        {
            get;
            set;
        }
        public string PwaCommandDescription
        {
            get
            {
                return "PwaGetResourcePlansCommand - gets resource plans";
            }
        }

        public string PwaCommandName
        {
            get
            {
                return "PwaGetResourcePlansCommand";
            }
        }

        public PwaResourcePlanInput PwaInput;
        private PJContext _pj;
        UpdateResult result = null;

        public void Execute()
        {
            try
            {
                var controller = new ResourcePlanController();
                controller.PJContext = _pj.PSI;
                OutputResult = GetResourcePlansForResource(_pj.PSI,PwaInput.ProjectUID, PwaInput.ProjectName, Convert.ToDateTime(PwaInput.StartDate), Convert.ToDateTime(PwaInput.EndDate),
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
            return new PwaGetResourcePlansCommand() { _pj = pj, PwaInput = (PwaResourcePlanInput)new PwaResourcePlanInput(pwaInput).ParseInput() };
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

        private ResPlan[] GetResourcePlansForResource(Microsoft.Office.Project.PWA.PSI psi, string puid, string projName, DateTime startDate, DateTime endDate, string timeScale,
           string workScale, bool isPSIVersion, string isCheckedOut, string checkOutBY)
        {
            try
            {
                var controller = new ResourcePlanController() { PJContext = psi };
                return controller.GetResourcePlan(puid, projName, startDate, endDate, timeScale, workScale, isPSIVersion, isCheckedOut, checkOutBY);
            }
            catch (Exception ex)
            {
                if (ex is SoapException)
                {
                    var error = new Microsoft.Office.Project.Server.Library.PSClientError(ex as SoapException);
                    var errors = error.GetAllErrors();
                    if (errors.Any(t => t.ErrId == Microsoft.Office.Project.Server.Library.PSErrorID.GeneralSecurityAccessDenied))
                    {
                        return new ResPlan[] { new ResPlan() { resource = new Resource() { resUid=Guid.Empty.ToString()}
                        ,projects=new Project[] { new Project(){ projUid=puid,projName=projName,intervals=new Intervals[] { } ,readOnly = true,
                            readOnlyReason ="You are not authorized to edit the resource plan for the project " + projName} }
                        }
                        };
                    }
                }
                return new ResPlan[] { new ResPlan() { resource = new Resource() { resUid=Guid.Empty.ToString()} } };

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
    public class ColumnModel
    {
        public string name { get; set; }
        public string index { get; set; }
        public string width { get; set; }
        public string align { get; set; }
        public bool editable { get; set; }
        public bool hidden { get; set; }
        public string formatter { get; set; }
        public string unformat { get; set; }
        public FormatOptions formatoptions { get; set; }
        public EditOptions editoptions { get; set; }
        public string edittype { get; set; }
        public bool sortable { get; set; }
        public string cellattr { get; set; }
        public SearchOptions searchoptions { get; set; }
        public ColumnModel()
        {
            searchoptions = new SearchOptions();
            searchoptions.sopt = new List<string>();
            searchoptions.sopt.Add("eq");
            searchoptions.sopt.Add("ne");
            searchoptions.sopt.Add("cn");
        }

        public FormOptions label { get; set; }
    }
    public class SearchOptions
    {
        public List<string> sopt;
    }
    public class EditOptions
    {
        public string value { get; set; }
        public string dataInit { get; set; }
    }
    public class FormatOptions
    {
        public string newformat { get; set; }
        public string dateformat { get; set; }
        public bool disabled { get; set; }
        public string baseLinkUrl { get; set; }

        public string showAction { get; set; }
    }
    public class FormOptions
    {
        public string label { get; set; }
    }

    public class ResourcePlanJSON
    {
        public List<Dictionary<string, Object>> ResourcePlan { get; set; }
        public List<string> columns { get; set; }
        public List<ColumnModel> columnModel { get; set; }
        public int TotalWidth
        {
            get
            {
                if (columns == null || columns.Count == 0 || columns.Count == 1)
                {
                    return 500;
                }
                if ((75 * (columns.Count - 1) + 210) > 1175)
                    return 1175;
                return (75 * (columns.Count - 1) + 210);
            }
        }
        public string ResourceName { get; set; }
    }
}



