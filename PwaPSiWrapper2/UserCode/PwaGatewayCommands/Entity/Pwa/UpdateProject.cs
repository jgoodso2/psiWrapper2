using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PwaPSIWrapper.UserCode.PwaGatewayCommands.Entity.Pwa
{
    public class UpdateProject
    {
        public string projUid { get; set; }
        public string projName { get; set; }

        public List<UpdateResource> resources { get; set; } = new List<UpdateResource>();
    }
}
