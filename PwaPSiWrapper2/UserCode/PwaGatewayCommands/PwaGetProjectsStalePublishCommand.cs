using Microsoft.Office.Project.PWA;
using PwaPSIWrapper.UserCode.PwaGatewayCommands.Entity;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;

namespace PwaPSIWrapper.UserCode.PwaGatewayCommands
{
    public class PwaGetProjectsStalePublishCommand : IPwaCommand, IPwaCommandFactory, IPwaOutput
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
                return "PwaGetProjectsStalePublishCommand - returns Project UIds that are in stale publish condition";
            }
        }

        public string PwaCommandName
        {
            get
            {
                return "PwaGetProjectsStalePublishCommand";
            }
        }

        public List<string> StaleProjects { get; private set; }

        public PwaGetProjectsStalePublishInput PwaInput;
        private PJContext _pj;
        

        public void Execute()
        {
            GetProjectsStalePublish(PwaInput.SelectedProject);
        
        }

        public IPwaCommand MakePwaCommand(PJContext pj, NameValueCollection pwaInput)
        {
            return new PwaGetProjectsStalePublishCommand() { _pj = pj, PwaInput = (PwaGetProjectsStalePublishInput)new PwaGetProjectsStalePublishInput(pwaInput).ParseInput() };
        }

        public void ProcessResult(HttpContext context)
        {
            Output = new JavaScriptSerializer().Serialize(StaleProjects);
        }

        private void GetProjectsStalePublish(string selectdProjects)
        {
            List<string> projects = new List<string>();
            DataTable dt = (DataTable)Newtonsoft.Json.JsonConvert.DeserializeObject(selectdProjects, (typeof(DataTable)));
            foreach (DataRow row in dt.Rows)
            {
                var projUID = new Guid(row.Field<string>("PROJ_UID"));
                var projName = row.Field<string>("PROJ_NAME");
                if (IsProjectStalePublish(projUID))
                {
                    projects.Add(projName);
                }
            }
            StaleProjects = projects;
        }

        private bool IsProjectStalePublish(Guid projUID)
        {
            return !(_pj.PSI.PWAWebService.ProjectGetProjectIsPublished(projUID) == 1);
        }
    }
}
