using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PwaPSIWrapper.UserCode.PwaGatewayCommands.Entity.Pwa
{
    public class ResPlan
    {
        public Resource resource { get; set; }
        public Project[] projects { get; set; }
        public bool selected { get; set; } 

       

        internal static UpdateResPlan[] GetUpdateResPlans(ResPlan[] resPlans)
        {
            var updateResPlans = new List<UpdateResPlan>();
           foreach(var resPlan in resPlans)
            {
                foreach(var project in resPlan.projects)
                {
                    if(updateResPlans.Any(p=>p.Project.projUid == project.projUid))
                    {
                        var existingProj = updateResPlans.First(p => p.Project.projUid == project.projUid);
                        var updateResource = new UpdateResource() { resource = resPlan.resource, intervals = project.intervals };
                        existingProj.Project.resources.Add(updateResource);
                    }
                    else
                    {
                        var updateResPlan = new UpdateResPlan() { Project = new UpdateProject() { projName = project.projName, projUid = project.projUid } } ;
                        var existingProj = updateResPlan;
                         var updateResource = new UpdateResource() { resource = resPlan.resource, intervals = project.intervals };
                        existingProj.Project.resources.Add(updateResource);
                        updateResPlans.Add(updateResPlan);
                    }
                }
            }
            return updateResPlans.ToArray();
        }
    }
}
