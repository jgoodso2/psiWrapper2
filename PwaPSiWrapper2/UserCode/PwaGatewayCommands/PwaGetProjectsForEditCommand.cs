
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Office.Project.PWA;
using PSLib = Microsoft.Office.Project.Server.Library;
using System.Data;
using System.Data.SqlClient;
using PwaPSIWrapper.UserCode.PwaGatewayCommands.Entity;
using System.Collections.Specialized;
using System.Web;
using System.Configuration;
using PwaPSIWrapper.Configuration;
using Newtonsoft.Json;
using PwaPSIWrapper.UserCode.PwaGatewayCommands.Entity.JSON;
using Project = PwaPSIWrapper.UserCode.PwaGatewayCommands.Entity.Pwa.Project;
using CustomField = PwaPSIWrapper.UserCode.PwaGatewayCommands.Entity.Pwa.CustomField;
using PwaPSIWrapper.UserCode.PwaGatewayCommands;

namespace PwaPSIWrapper
{
    public class PwaGetProjectsForEditCommand : IPwaCommand, IPwaCommandFactory, IPwaOutput
    {
        PJContext _pj;

        public string PwaCommandDescription
        {
            get { return "PwaPublish command - needs a ProjectUID"; }
        }

        public string PwaCommandName
        {
            get { return "PwaGetProjectsForEditCommand"; }
            set { PwaCommandName = value; }
        }

        public string Output
        {
            get; set;
        }

        public Project[] OutputDataSet { get; set; }
        public PwaResourcePlanInput PwaInput;

        public void Execute()
        {
            //OutputDataSet = new DataTable();
            var projects = _pj.PSI.ProjectWebService.ReadProjectStatus(Guid.Empty, PSLib.DataStoreEnum.WorkingStore, string.Empty, (int)PSLib.Project.ProjectType.Project);
            string[] columnsCopy = new string[0];
            OutputDataSet = GetProjectsCustomFields();


        }

        Project[] GetProjectsCustomFields()
        {
            var gridSerializer = new Microsoft.Office.Project.Server.Utility.JsGrid.JsGridSerializerArguments();
            //TODO View Guid to be moved in config
            //dev
            //var ds = this._pj.PSI.PWAWebService.ProjectGetProjectCenterProjectsForGridJson(gridSerializer
            //    , new Guid("63d3499e-df27-401c-af58-ebb9607beae8"), 1, true, true);
            //qa
            var ds = this._pj.PSI.PWAWebService.ProjectGetProjectCenterProjectsForGridJson(gridSerializer
                , new Guid(PwaInput.ViewGuid), 1, true, true);

            Newtonsoft.Json.Linq.JObject o = Newtonsoft.Json.Linq.JObject.Parse(ds);

            var fields = o["AdditionalParams"]["PropertyManager"]["properties"]["Fields"]["value"].ToObject<Item[]>();
            var customFieldMap = fields.ToDictionary(t => t.sQLName, t => t.name);
            List<Project> projects = new List<Project>();
            foreach (var value in o["UnlocalizedTable"].Children())
            {
                var project = new Project();
                project.projUid = value["PROJ_UID"].ToString();
                project.projName = value["PROJ_NAME"].ToString();
                projects.Add(project);
            }
            foreach (var value in o["LocalizedTable"].Children())
            {
                NameValueCollection collection = new NameValueCollection();
                var project = projects.First(p => p.projName == value["PROJ_NAME"].ToString());
                project.CustomFields = new CustomField[customFieldMap.Keys.Count];
                var counter = 0;
                foreach (var prop in customFieldMap.Keys)
                {
                    project.CustomFields[counter++] = new CustomField() { Name = customFieldMap[prop], Value = value[prop] == null ? "" : value[prop].ToString() };
                }
                //.Add(collection);
            }
            return projects.ToArray();

        }
        public IPwaCommand MakePwaCommand(PJContext pj, NameValueCollection args)
        {
            return new PwaGetProjectsForEditCommand() { _pj = pj, PwaInput = (PwaResourcePlanInput)new PwaResourcePlanInput(args).ParseInput() };
        }

        public void ProcessResult(HttpContext context)
        {
            Output = Newtonsoft.Json.JsonConvert.SerializeObject(OutputDataSet);
        }


        #region scrap


        #endregion


    }
}
