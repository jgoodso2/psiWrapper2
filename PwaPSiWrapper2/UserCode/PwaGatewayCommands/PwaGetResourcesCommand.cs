
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
    using Resource = PwaPSIWrapper.UserCode.PwaGatewayCommands.Entity.Pwa.Resource;
using CustomField = PwaPSIWrapper.UserCode.PwaGatewayCommands.Entity.Pwa.CustomField;
using PwaPSIWrapper.UserCode.PwaGatewayCommands;

namespace PwaPSIWrapper
{
    public class PwaGetResourcesCommand : IPwaCommand, IPwaCommandFactory, IPwaOutput
    {
        PJContext _pj;

        public string PwaCommandDescription
        {
            get { return "PwaPublish command - needs a ProjectUID"; }
        }

        public string PwaCommandName
        {
            get { return "PwaGetResourcesCommand"; }
            set { PwaCommandName = value; }
        }

        public string Output
        {
            get; set;
        }

        public Resource[] OutputDataSet { get; set; }
        public PwaResourcePlanInput PwaInput;

        public void Execute()
        {
            //OutputDataSet = new DataTable();
            var projects = _pj.PSI.ProjectWebService.ReadProjectStatus(Guid.Empty, PSLib.DataStoreEnum.WorkingStore, string.Empty, (int)PSLib.Project.ProjectType.Project);
            string[] columnsCopy = new string[0];
            OutputDataSet = GetResourcesCustomFields();


        }

        Resource[] GetResourcesCustomFields()
        {
            var gridSerializer = new Microsoft.Office.Project.Server.Utility.JsGrid.JsGridSerializerArguments();
            //TODO View Guid to be moved in config
            var ds = this._pj.PSI.PWAWebService.ResourceGetResourceCenterResourcesForGridJson(gridSerializer
                , new Guid(PwaInput.ViewGuid), 1, "", true);
            Newtonsoft.Json.Linq.JObject o = Newtonsoft.Json.Linq.JObject.Parse(ds);

            var fields = o["AdditionalParams"]["PropertyManager"]["properties"]["Fields"]["value"].ToObject<Item[]>();
            var customFieldMap = fields.ToDictionary(t => t.sQLName, t => t.name);
            List<Resource> resources = new List<Resource>();
            foreach (var value in o["UnlocalizedTable"].Children())
            {
                var resource = new Resource();
                resource.resUid = value["RES_UID"].ToString();
                resource.resName = value["RES_NAME"].ToString();
                resources.Add(resource);
            }
            foreach (var value in o["LocalizedTable"].Children())
            {
                NameValueCollection collection = new NameValueCollection();
                var resource = resources.First(p => p.resName == value["RES_NAME"].ToString());
                resource.CustomFields = new CustomField[customFieldMap.Keys.Count];
                var counter = 0;
                foreach (var prop in customFieldMap.Keys)
                {
                    resource.CustomFields[counter++] = new CustomField() { Name = customFieldMap[prop], Value = value[prop] == null ? "" : value[prop].ToString() };
                }
                //.Add(collection);
            }

            return resources.ToArray();

        }
        public IPwaCommand MakePwaCommand(PJContext pj, NameValueCollection  args)
        {
            return new PwaGetResourcesCommand() { _pj = pj, PwaInput = (PwaResourcePlanInput) new PwaResourcePlanInput(args).ParseInput() };
        }

        public void ProcessResult(HttpContext context)
        {
            Output = Newtonsoft.Json.JsonConvert.SerializeObject(OutputDataSet);
        }


        #region scrap


        #endregion


    }
}
