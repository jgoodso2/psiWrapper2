using PwaPSIWrapper.UserCode.Misc;
using System.Web;

namespace PwaPSIWrapper.UserCode.PwaGatewayCommands.Entity
{
    public interface IPwaOutput
    {
       string Output { get; set; }

        void ProcessResult(HttpContext context);
    }
}
