using Microsoft.Office.Project.PWA;
using PwaPSIWrapper.UserCode.PwaGatewayCommands.Entity;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Web;
using System.Web.Script.Serialization;
using PJSchema = Microsoft.Office.Project.Server.Schema;

namespace PwaPSIWrapper.UserCode.PwaGatewayCommands
{
    public  class PwaGetCheckedoutProjectsCommand : IPwaCommand, IPwaCommandFactory, IPwaOutput
    {
        public string Output
        {
            get;
            set;
        }

        public string PwaCommandDescription
        {
            get
            {
                return "PwaGetCheckedoutProjectsCommand - returns Project UIds that are in checked out condition";
            }
        }

        public string PwaCommandName
        {
            get
            {
                return "PwaGetCheckedoutProjectsCommand";
            }
        }

        public List<CheckedOutInfo> CheckedoutProjects { get; private set; }

        public PwaGetProjectsCheckedoutInput PwaInput;
        private PJContext _pj;


        public void Execute()
        {
            GetProjectsCheckedout(PwaInput.SelectedProject);

        }

        public IPwaCommand MakePwaCommand(PJContext pj, NameValueCollection pwaInput)
        {
            return new PwaGetCheckedoutProjectsCommand() { _pj = pj, PwaInput = (PwaGetProjectsCheckedoutInput)new PwaGetProjectsCheckedoutInput(pwaInput).ParseInput() };
        }

        public void ProcessResult(HttpContext context)
        {
            Output = new JavaScriptSerializer().Serialize(CheckedoutProjects);
        }

        private void GetProjectsCheckedout(string selectdProjects)
        {
            List<CheckedOutInfo> projects = new List<CheckedOutInfo>();
            DataTable dt = (DataTable)Newtonsoft.Json.JsonConvert.DeserializeObject(selectdProjects, (typeof(DataTable)));
            foreach (DataRow row in dt.Rows)
            {
                var projUID = new Guid(row.Field<string>("PROJ_UID"));
                var projName = row.Field<string>("PROJ_NAME");
                CheckedOutInfo info = GetCheckedOutInfo(projUID);
                if (!string.IsNullOrEmpty(info.User))
                {
                    projects.Add(info);
                }
            }
            CheckedoutProjects = projects;
        }

        public CheckedOutInfo GetCheckedOutInfo(Guid projUID)
        {
            CheckedOutInfo info = new CheckedOutInfo() { User = "", PROJ_NAME = "" };
            var checkedOutPlans = _pj.PSI.PWAWebService.AdminReadCheckedOutEnterpriseResourcePlans();
            foreach (PJSchema.AdminCheckedOutResourcePlansDataSet.CheckedOutResourcePlansRow row in checkedOutPlans.CheckedOutResourcePlans)
            {
                if (row.PROJ_UID == projUID)
                {
                    info.PROJ_NAME = row.PROJ_NAME;
                    info.User = row.RES_NAME;
                    break;
                }
            }
            return info;
        }
    }

    public class CheckedOutInfo
    {
        public string PROJ_NAME { get; set; }
        public string User { get; set; }
    }
}
