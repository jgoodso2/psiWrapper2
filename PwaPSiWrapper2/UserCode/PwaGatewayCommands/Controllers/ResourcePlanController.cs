using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Data;
using System.Web.Script.Serialization;
using PSLib = Microsoft.Office.Project.Server.Library;
using PJSchema = Microsoft.Office.Project.Server.Schema;
using PSPJLib = Microsoft.Office.Project.PWA;
using System.Configuration;
using System.Linq;
using System.Data.SqlClient;
using PwaPSIWrapper.UserCode.PwaGatewayCommands.Entity.Pwa;
using System.Data;
using PwaPSIWrapper.UserCode.Utility;

namespace PwaPSIWrapper.UserCode.PwaGatewayCommands
{
    public class ResourcePlanController
    {
        public ResPlan[] GetResourcePlan(string puid, string projName, DateTime startDate, DateTime endDate, string timeScale,
            string workscale, bool ispsiVersion, string isCheckedOut, string checkOutBY)
        {
            ResourcePlanRepository repository = new ResourcePlanRepository();
            repository.PJPSIContext = PJContext;
            return repository.GetResourcePlan(startDate, endDate, timeScale, workscale, new Guid(puid), projName, false, isCheckedOut, checkOutBY);
        }

        public bool CheckoutProjectPlan(string projectUID)
        {
            ResourcePlanRepository repository = new ResourcePlanRepository();
            return repository.CheckoutProjectPlan(projectUID);
        }
        public UpdateResult UpdateResourcePlan(UpdateResource[] resources, string puid, string timeScale, string workScale, string startDate, string endDate)
        {
            UpdateResult result = new UpdateResult() { project = new Project() { projUid = puid } };
            try
            {
                PJSchema.ResourcePlanDataSet dataSet = new PJSchema.ResourcePlanDataSet();

                DateTime sDate = DateTime.Parse(startDate);
                DateTime eDate = DateTime.Parse(endDate);

                ResourcePlanRepository repository = new ResourcePlanRepository();
                repository.PJPSIContext = PJContext;
                dataSet = repository.GetResourcePlan("", "",
                    ref sDate, ref eDate,
                    timeScale, workScale, new Guid(puid), true, "0", "");

                

                foreach (var resource in resources)
                {
                    if(!dataSet.PlanResources.AsEnumerable().Any(t => t.Field<Guid>("RES_UID").ToString().ToUpper() == resource.resource.resUid.ToUpper()))
                    {
                        result.success = false;
                        result.error = string.Format(" The resource {0} is deleted from this project  and is no longer avialable for update. Please refresh the page.", resource.resource.resName);
                        return result;
                    }
                    var planResRow = (PJSchema.ResourcePlanDataSet.PlanResourcesRow)dataSet.PlanResources.AsEnumerable().First(t => t.Field<Guid>("RES_UID").ToString().ToUpper() == resource.resource.resUid.ToUpper());
                    foreach (PJSchema.ResourcePlanDataSet.DatesRow interval in dataSet.Dates)
                    {
                        if (workScale.ToUpper() == "HOURS")
                        {
                            if (!string.IsNullOrWhiteSpace(resource[interval.IntervalName]))
                            {
                                planResRow[interval.IntervalName] = Convert.ToDouble(resource[interval.IntervalName].Replace("h", "")) / 8 * 4800;
                            }
                        }

                        else if (workScale.ToUpper() == "DAYS")
                        {
                            if (!string.IsNullOrWhiteSpace(resource[interval.IntervalName]))
                            {
                                planResRow[interval.IntervalName] = Convert.ToDouble(resource[interval.IntervalName].Replace("d", "")) * 4800;
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrWhiteSpace(resource[interval.IntervalName]))
                            {
                                planResRow[interval.IntervalName] = Convert.ToDouble(resource[interval.IntervalName].Replace("%", ""));
                            }
                        }
                    }
                }

                //if (timeScale == "Financial Months" && workScale.ToLower() != "hours")
                //{
                //    dataSet = BuildDataSetForDay(dataSet, DateTime.Parse(startDate), DateTime.Parse(endDate), ruid);
                //}
                return repository.UpdateResourcePlan(dataSet ,false,puid,timeScale, workScale);

            }
            catch (Exception ex)
            {
                result.error = "An unexpected error occured in Save.Please contact the system administrator";
               result.debugError = ex.Message;
            }

            return result;
        }

        private PJSchema.ResourcePlanDataSet BuildDataSetForDay(PJSchema.ResourcePlanDataSet dataSet, DateTime startDate, DateTime endDate, string ruid)
        {
            lock (HttpContext.Current.Application)
            {

                ResourcePlanRepository repository = new ResourcePlanRepository() { PJPSIContext = PJContext };
                DataTable allPeriods = repository.GetFinancialPeriods(startDate, endDate);
                PJSchema.ResourcePlanDataSet.DatesDataTable financialPeriods = repository.GetFinancialPeriods(allPeriods);
                var sDate = financialPeriods[0].StartDate;
                var eDate = financialPeriods[financialPeriods.Count - 1].EndDate;
                var timePhasedData = repository.GetFinanacialTimePhasedData(ruid, sDate, eDate);
                Microsoft.Office.Project.Server.Library.Filter filter = new Microsoft.Office.Project.Server.Library.Filter();
                filter.FilterTableName = dataSet.PlanResources.TableName;
                Microsoft.Office.Project.Server.Library.Filter.FieldOperator op =
                    new Microsoft.Office.Project.Server.Library.Filter.FieldOperator(Microsoft.Office.Project.Server.Library.Filter.FieldOperationType.Equal,
                   dataSet.PlanResources.RES_UIDColumn.ColumnName, dataSet.PlanResources[0].RES_UID);
                filter.Criteria = op;

                PJSchema.ResourcePlanDataSet dayDataSet = PJContext.ResourcePlanWebService.ReadResourcePlan(filter.GetXml(), dataSet.PlanResources[0].PROJ_UID, startDate, endDate, 3, false, false);
                int i = 0;

                foreach (PJSchema.ResourcePlanDataSet.DatesRow datesRow in dataSet.Dates)
                {
                    double value = dataSet.PlanResources[0][datesRow.IntervalName] != System.DBNull.Value ? Convert.ToSingle(dataSet.PlanResources[0][datesRow.IntervalName]) * 600 : 0.0;


                    for (DateTime start = datesRow.StartDate; start <= datesRow.EndDate; start = start.AddDays(1))
                    {

                        double dayValue = value * (double)timePhasedData[start.ToShortDateString()];
                        dayDataSet.PlanResources[0]["Interval" + i.ToString()] = dayValue;
                        i++;
                    }
                }
                dayDataSet.Dates.AcceptChanges();
                return dayDataSet;
            }

        }

        public UpdateResult PublishResourcePlan(DataRow plan, string user, string ruid, string timeScale, string workScale, string startDate, string endDate)
        {

            PJSchema.ResourcePlanDataSet dataSet = new PJSchema.ResourcePlanDataSet();
            var result = new UpdateResult() { project = new Project() };
            result.project.projUid = plan.Field<string>("PROJ_UID");
            result.project.projUid = plan.Field<string>("PROJ_NAME");
            var sDate = DateTime.Parse(startDate);
            var eDate = DateTime.Parse(endDate);
            if (plan.Field<bool>("isDirty") == false)
            {
                result.success = true;
                return result;
            }
            ResourcePlanRepository repository = new ResourcePlanRepository();
            repository.PJPSIContext = PJContext;
            
            dataSet = repository.GetResourcePlan(user, ruid,
                ref sDate, ref eDate,
                timeScale, workScale, new Guid(plan.Field<string>("PROJ_UID")), true, "0", "");

            foreach (PJSchema.ResourcePlanDataSet.DatesRow interval in dataSet.Dates)
            {
                if (workScale.ToUpper() == "HOURS")
                {
                    if (plan[interval.IntervalName] != null && plan[interval.IntervalName] != System.DBNull.Value)
                    {
                        dataSet.PlanResources[0][interval.IntervalName] = Convert.ToDouble(plan[interval.IntervalName]) * 600;
                    }
                }
                else
                {
                    if (plan[interval.IntervalName] != null && plan[interval.IntervalName] != System.DBNull.Value)
                    {
                        dataSet.PlanResources[0][interval.IntervalName] = Convert.ToDouble(plan[interval.IntervalName]);
                    }
                }
            }
            return repository.PublishResourcePlan(dataSet, plan.Field<string>("PROJ_UID"), Guid.NewGuid());
        }

        public PSPJLib.PSI PJContext { get; set; }

        internal string GetProjects(List<string> projects, List<string> columns)
        {
            ResourcePlanRepository repository = new ResourcePlanRepository();
            repository.PJPSIContext = PJContext;
            List<Dictionary<string, Object>> resources = repository.GetProjects(projects, columns);
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            serializer.MaxJsonLength = Int32.MaxValue;
            return serializer.Serialize(resources);

        }
        public bool IsprojectStalePublish(Guid projuid)
        {
            ResourcePlanRepository repository = new ResourcePlanRepository();
            repository.PJPSIContext = PJContext;
            return repository.IsprojectStalePublish(projuid);
        }

        internal UpdateResult AddResourcePlan(string projectUID, string projectName,string resourceUID, string timeScale, string workScale, string startDate, string endDate)
        {
            ResourcePlanRepository repository = new ResourcePlanRepository();
            repository.PJPSIContext = PJContext;
            return repository.AddResourcePlan(projectUID, projectName, resourceUID, timeScale, workScale, startDate, endDate);
        }

        internal UpdateResult DeleteResourcePlan(UpdateResPlan resPlan, string timeScale, string workScale, string startDate, string endDate)
        {
            ResourcePlanRepository repository = new ResourcePlanRepository();
            repository.PJPSIContext = PJContext;
            return repository.DeleteResourcePlan(resPlan, timeScale, workScale, startDate, endDate);
        }

        internal DataTable GetProjectsWithResourcePlansForResource(string ruid)
        {
            ResourcePlanRepository repository = new ResourcePlanRepository();
            repository.PJPSIContext = PJContext;
            var projects = repository.GetProjectsWithResourcePlansForResource(ruid);
            return projects;
        }

        internal CheckedOutInfo GetCheckedOutInfo(Guid projUID)
        {
            ResourcePlanRepository repository = new ResourcePlanRepository();
            repository.PJPSIContext = PJContext;
            return repository.GetCheckedOutInfo(projUID);
        }
    }
}