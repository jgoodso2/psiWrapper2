using System;
using Microsoft.Office.Project.PWA;
using System.Web;
using Microsoft.SharePoint.Utilities;
using System.Collections.Generic;
using PwaPSIWrapper.UserCode.PwaGatewayCommands.Entity;
using Newtonsoft.Json;
using PwaPSIWrapper.UserCode.Misc;
using System.Linq;
using System.Web.Script.Serialization;
using PwaPSIWrapper.UserCode.PwaGatewayCommands;
using PwaPSIWrapper;

namespace PwaPSiWrapper2.Layouts.PwaPSiWrapper2
{
    public partial class PwaAdapter : PJWebPage
    {
        protected void Page_PreRender(object sender, EventArgs e)
        {
            var availcmds = CreatePwaCommands();
            var _parser = new PwaCommandParser(this.PjContext, availcmds);

            var cmd = _parser.ParseCommand(Request.Form);

            cmd.Execute();

            if (cmd is IPwaOutput)
            {
                (cmd as IPwaOutput).ProcessResult(HttpContext.Current);
                Response.ContentType = "text/plain";
                Response.Write((cmd as IPwaOutput).Output);
                Response.Flush(); // Sends all currently buffered output to the client.
                Response.SuppressContent = true;  // Gets or sets a value indicating whether to send HTTP content to the client.
                HttpContext.Current.ApplicationInstance.CompleteRequest();
            }
        }

        private IEnumerable<IPwaCommandFactory> CreatePwaCommands()
        {

            IPwaCommandFactory PublishCommand = new PwaPublishCommand();
            IPwaCommandFactory NotFoundCommand = new PwaNotFoundCommand();
            IPwaCommandFactory getProjectsCommand = new PwaGetProjectsForEditCommand();
            IPwaCommandFactory updateProjectsCommand = new PwaUpdateProjectsCustomFieldsCommand();
            IPwaCommandFactory getResourcePlansCommand = new PwaGetResourcePlansCommand();
            IPwaCommandFactory getprojectUidsCommand = new PwaGetProjectUidsCommand();
            IPwaCommandFactory getcheckedoutProjectsCommand = new PwaGetCheckedoutProjectsCommand();
            IPwaCommandFactory getProjectsStalePublishCommand = new PwaGetProjectsStalePublishCommand();
            IPwaCommandFactory publishResourcePlanCommand = new PwaPublishResourcePlanCommand();
            IPwaCommandFactory updateResourcePlanCommand = new PwaUpdateResourcePlanCommand();
            IPwaCommandFactory addResourcePlanCommand = new PwaAddResourcePlanCommand();
            IPwaCommandFactory deleteResourcePlanCommand = new PwaDeleteResourcePlanCommand();
            IPwaCommandFactory getResourcesCommand = new PwaGetResourcesCommand();
            List<IPwaCommandFactory> commands = new List<IPwaCommandFactory>();
            commands.Add(PublishCommand);
            commands.Add(NotFoundCommand);
            commands.Add(getProjectsCommand);
            commands.Add(updateProjectsCommand);
            commands.Add(getResourcePlansCommand);
            commands.Add(getprojectUidsCommand);
            commands.Add(getcheckedoutProjectsCommand);
            commands.Add(getProjectsStalePublishCommand);
            commands.Add(publishResourcePlanCommand);
            commands.Add(updateResourcePlanCommand);
            commands.Add(addResourcePlanCommand);
            commands.Add(deleteResourcePlanCommand);
            commands.Add(getResourcesCommand);
            return commands;

        }
    }
}
