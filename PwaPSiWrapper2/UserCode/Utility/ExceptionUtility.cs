using Microsoft.Office.Project.Server.Library;
using PwaPSIWrapper.UserCode.PwaGatewayCommands;
using PwaPSIWrapper.UserCode.PwaGatewayCommands.Entity.Pwa;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Services.Protocols;

namespace PwaPSIWrapper.UserCode.Utility
{
    public class ExceptionUtility
    {
       public static void HandleException(Exception  ex, string projUid,string projName,out UpdateResult result)
        {
            result = new UpdateResult();
            if (ex is SoapException)
            {
                var error = new PSClientError(ex as SoapException);
                var errors = error.GetAllErrors();
                if (errors.Any(t => t.ErrId == PSErrorID.CICOCheckedOutToOtherUser))
                {
                    result.success = false;
                    result.project = new PwaPSIWrapper.UserCode.PwaGatewayCommands.Entity.Pwa.Project() { projName = projName, projUid = projUid };
                    result.error = "Checked to other user";
                    result.debugError = ex.Message;
                }
                else if (errors.Any(t => t.ErrId == PSErrorID.CICOAlreadyCheckedOutToYou))
                {
                    result.success = false;
                    result.project = new PwaPSIWrapper.UserCode.PwaGatewayCommands.Entity.Pwa.Project() { projName = projName, projUid = projUid };
                    result.error = "Checked out to you in another session";
                    result.debugError = ex.Message;
                }

                else if (errors.Any(t => t.ErrId == PSErrorID.CICOAlreadyCheckedOutInSameSession))
                {
                    result.success = false;
                    result.project = new PwaPSIWrapper.UserCode.PwaGatewayCommands.Entity.Pwa.Project() { projName = projName, projUid = projUid };
                    result.error = "Checked out to you in another session";
                    result.debugError = ex.Message;
                }

                else if (errors.Any(t => t.ErrId == PSErrorID.ResourcePlanCheckinFailure))
                {
                    result.success = false;
                    result.project = new PwaPSIWrapper.UserCode.PwaGatewayCommands.Entity.Pwa.Project() { projName = projName, projUid = projUid };
                    result.error = " Failed to check in resource plan for " + projName;
                    result.debugError = ex.Message;
                }
                else if (errors.Any(t => t.ErrId == PSErrorID.ResourcePlanDeleteFailure))
                {
                    result.success = false;
                    result.project = new PwaPSIWrapper.UserCode.PwaGatewayCommands.Entity.Pwa.Project() { projName = projName, projUid = projUid };
                    result.error = " Failed to delete resource plan for " + projName;
                    result.debugError = ex.Message;
                }
                else if (errors.Any(t => t.ErrId == PSErrorID.ResourcePlanPublishFailure))
                {
                    result.success = false;
                    result.project = new PwaPSIWrapper.UserCode.PwaGatewayCommands.Entity.Pwa.Project() { projName = projName, projUid = projUid };
                    result.error = " Failed to publish resource plan for " + projName;
                    result.debugError = ex.Message;
                }

                else if (errors.Any(t => t.ErrId == PSErrorID.ResourcePlanSaveFailure))
                {
                    result.success = false;
                    result.project = new PwaPSIWrapper.UserCode.PwaGatewayCommands.Entity.Pwa.Project() { projName = projName, projUid = projUid };
                    result.error = " Falied to save resource plan for " + projName;
                    result.debugError = ex.Message;
                }
                else
                {
                    result.success = false;
                    result.project = new PwaPSIWrapper.UserCode.PwaGatewayCommands.Entity.Pwa.Project() { projName = projName, projUid = projUid };
                    result.error = "An unexpected error occured for resource plan " + projName;
                    result.debugError = ex.Message;
                }
            }
        }
    }
}
