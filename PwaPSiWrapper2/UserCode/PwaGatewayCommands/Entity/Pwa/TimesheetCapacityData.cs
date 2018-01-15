using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PwaPSiWrapper2.UserCode.PwaGatewayCommands.Entity.Pwa
{
    public class TimesheetCapacityData
    {
        public decimal Capacity { get; set; }
        public Dictionary<Guid, decimal> TimesheetData {get;set;}
    }
}
