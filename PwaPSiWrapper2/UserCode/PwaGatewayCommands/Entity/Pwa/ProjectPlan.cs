using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PwaPSIWrapper.UserCode.PwaGatewayCommands.Entity.Pwa
{
    public class ProjectPlan
    {

        public Resource[] resources { get; set; }
        public Project project { get; set; }
        public bool selected { get; set; }

        internal static UpdateResPlan[] GetUpdateResPlans(ProjectPlan[] rp)
        {
            List<UpdateResPlan> resPlans = new List<UpdateResPlan>();
            foreach(var plan in rp)
            {
                UpdateResPlan updatePlan = new UpdateResPlan()
                {
                    Project = new UpdateProject()
                    {
                        projUid = plan.project.projUid,
                        projName = plan.project.projName
                    ,
                        resources = plan.resources.Select(r => new UpdateResource() { resource = r, intervals = r.intervals }).ToList()
                    }
                };
                resPlans.Add(updatePlan);
            }
            return resPlans.ToArray();
        }
    }
}
