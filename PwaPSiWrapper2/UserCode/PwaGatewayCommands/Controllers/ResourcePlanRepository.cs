using Microsoft.Office.Project.Server.Library;
using PwaPSIWrapper.Configuration;
using PwaPSIWrapper.UserCode.PwaGatewayCommands.Entity.Pwa;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Services.Protocols;
using static Microsoft.Office.Project.Server.Library.TimeScaleClass;
using PJSchema = Microsoft.Office.Project.Server.Schema;
using PSLib = Microsoft.Office.Project.Server.Library;
using PSPJLib = Microsoft.Office.Project.PWA;
using Project = PwaPSIWrapper.UserCode.PwaGatewayCommands.Entity.Pwa.Project;
using Resource = PwaPSIWrapper.UserCode.PwaGatewayCommands.Entity.Pwa.Resource;
using PwaPSIWrapper.UserCode.Utility;

namespace PwaPSIWrapper.UserCode.PwaGatewayCommands
{
    public class ResourcePlanRepository
    {
        public PSPJLib.PSI PJPSIContext
        {
            get;
            set;
        }
        public PJSchema.ProjectDataSet ReadStatStatus()
        {
            return ReadProjectsList();
        }

        public PJSchema.ProjectDataSet ReadProjectsList()  //called by configuration screen
        {
            PJSchema.ProjectDataSet projectList = new PJSchema.ProjectDataSet();
            try
            {
                // Get projects of type normal, templates, proposals, master, and inserted.
                string projectName = string.Empty;

                Dictionary<Guid, string> projects = new Dictionary<Guid, string>();

                projectList = PJPSIContext.ProjectWebService.ReadProjectStatus(Guid.Empty, PSLib.DataStoreEnum.PublishedStore,
                    string.Empty, (int)PSLib.Project.ProjectType.Project);




            }
            catch (Exception ex)
            {

            }
            finally
            {

            }

            return projectList;
        }

        public Dictionary<string, Decimal> GetFinanacialTimePhasedData(string resUID, DateTime? startDate, DateTime? endDate)
        {
            lock (HttpContext.Current.Application)
            {
                //if (startDate.HasValue && endDate.HasValue)
                //{
                //    if (HttpContext.Current.Application["timephase" + startDate.Value.ToShortDateString() + endDate.Value.ToShortDateString()] != null)
                //    {
                //        return HttpContext.Current.Application["timephase" + startDate.Value.ToShortDateString() + endDate.Value.ToShortDateString()] as Dictionary<string, Decimal>;
                //    }
                //}
                DataTable dt = new DataTable();
                SqlConnection connection = new SqlConnection(ConfigurationUtility.GetConnectionString("DataMart"));
                SqlCommand command = new SqlCommand();
                command.Connection = connection;
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = 60;
                command.CommandText = "usp_GetFinancialTimePhasedData";
                command.Parameters.Add("@StartDate", SqlDbType.DateTime);
                command.Parameters.Add("@EndDate", SqlDbType.DateTime);
                if ((startDate.HasValue && endDate.HasValue))
                {
                    command.Parameters["@StartDate"].Value = startDate.Value;

                    command.Parameters["@EndDate"].Value = endDate.Value;
                }
                else
                {
                    command.Parameters["@StartDate"].Value = System.DBNull.Value;

                    command.Parameters["@EndDate"].Value = System.DBNull.Value;
                }
                command.Parameters.Add("@ResourceUID", SqlDbType.UniqueIdentifier);
                command.Parameters["@ResourceUID"].Value = new Guid(resUID);
                SqlDataAdapter adapter = new SqlDataAdapter();
                adapter.SelectCommand = command;
                adapter.Fill(dt);

                //if (startDate.HasValue && endDate.HasValue)
                //{
                //    HttpContext.Current.Application[startDate.Value.ToShortDateString() + endDate.Value.ToShortDateString()] = dt;
                //}
                return dt.AsEnumerable().ToDictionary(t => t.Field<DateTime>("TimeByDay").ToShortDateString(), t => t.Field<decimal>("Capacity"));
            }
        }

        public PJSchema.ResourcePlanDataSet GetResourcePlan(string user, string resUID, ref DateTime sDate, ref DateTime eDate,
           string timeScale, string workScale, Guid projectUiid, bool isPSIVersion, string isCheckedOut, string checkOutBY)
        {
            try
            {
                PJSchema.ResourcePlanDataSet rds = new PJSchema.ResourcePlanDataSet();
                Microsoft.Office.Project.Server.Library.Filter filter = new Microsoft.Office.Project.Server.Library.Filter();
                filter.FilterTableName = rds.PlanResources.TableName;
                Microsoft.Office.Project.Server.Library.Filter.FieldOperator op =
                    new Microsoft.Office.Project.Server.Library.Filter.FieldOperator(Microsoft.Office.Project.Server.Library.Filter.FieldOperationType.Equal,
                   rds.PlanResources.RES_UIDColumn.ColumnName, resUID);
                filter.Criteria = op;

                short intTimeScale = 5;
                switch (timeScale)
                {
                    case "Weeks":
                        intTimeScale = 4;
                        break;
                    case "Calendar Months":
                        intTimeScale = 5;
                        break;
                    case "Financial Months":
                        intTimeScale = 5;
                        break;
                    case "Years":
                        intTimeScale = 7;
                        break;
                }
                if (timeScale == "Financial Months")
                {

                    DataTable allPeriods = GetFinancialPeriods(sDate, eDate);

                    PJSchema.ResourcePlanDataSet.DatesDataTable financialPeriods = GetFinancialPeriods(allPeriods);
                    sDate = financialPeriods[0].StartDate;
                    eDate = financialPeriods[financialPeriods.Count - 1].EndDate;
                    PJSchema.ResourcePlanDataSet ds = PJPSIContext.ResourcePlanWebService.ReadResourcePlan(string.Empty,
                        projectUiid
                        , sDate, eDate, 3, workScale.ToUpper() == "FTE", false);

                    PJSchema.ResourcePlanDataSet dsCopy = new PJSchema.ResourcePlanDataSet();
                    PJSchema.ResourcePlanDataSet.PlanResourcesDataTable planTable = dsCopy.PlanResources;

                    var planRow = planTable.NewPlanResourcesRow();
                    dsCopy.Dates.Clear();
                    foreach (DataRow interval in financialPeriods.Rows)
                    {
                        dsCopy.Dates.ImportRow(interval);
                    }
                    Dictionary<DateTime, string> IntervalDict = ds.Dates.AsEnumerable().ToDictionary(t => t.Field<DateTime>("StartDate").Date, t => t.Field<string>("IntervalName"));

                    for (int i = 0; i < ds.PlanResources.Columns.Count; i++)
                    {
                        if (!ds.PlanResources.Columns[i].ColumnName.StartsWith("Interval"))
                        {
                            if (!planTable.Columns.Contains(ds.PlanResources.Columns[i].ColumnName))
                            {
                                planTable.Columns.Add(ds.PlanResources.Columns[i].ColumnName, ds.PlanResources.Columns[i].DataType);
                            }
                            planRow[i] = ds.PlanResources[0][i];
                        }

                    }

                    planRow.ASSN_BOOKING_TYPE = ds.PlanResources[0].ASSN_BOOKING_TYPE;
                    planTable.AddPlanResourcesRow(planRow);
                    Dictionary<string, decimal> finanacialPeriodsTimePhasedData = GetFinanacialTimePhasedData(resUID, sDate, eDate);
                    /* if(finanacialPeriodsTimePhasedData.Keys.Count < (eDate - sDate).Days)
                     {
                         finanacialPeriodsTimePhasedData = GetFinanacialTimePhasedData(resUID, null, null);
                         if(finanacialPeriodsTimePhasedData.Keys.Count < 1)
                         {
                             throw new Exception("Resource is not available to work. Please schedule the resource availabilty or contact the administrator");
                         }
                         else
                         {
                             DateTime minDate = finanacialPeriodsTimePhasedData.Keys.Min(t=>Convert.ToDateTime(t));
                             DateTime maxDate = finanacialPeriodsTimePhasedData.Keys.Max(t => Convert.ToDateTime(t));
                             for (int i=0;i<financialPeriods.Rows.Count;i++)
                             {
                                 if (minDate == financialPeriods.Rows[i].Field<DateTime>("StartDate"))
                                 {
                                     minDate = financialPeriods.Rows[i].Field<DateTime>("StartDate");
                                     break;
                                 }
                                 if(minDate > financialPeriods.Rows[i].Field<DateTime>("StartDate"))
                                 {
                                     minDate = financialPeriods.Rows[i].Field<DateTime>("EndDate");
                                     break;
                                 }
                             }

                             for (int i = financialPeriods.Rows.Count - 1; i >= 0; i--)
                             {
                                 if (maxDate == financialPeriods.Rows[i].Field<DateTime>("EndDate"))
                                 {
                                     maxDate = financialPeriods.Rows[i].Field<DateTime>("EndDate");
                                     break;
                                 }

                                 if (maxDate < financialPeriods.Rows[i].Field<DateTime>("EndDate"))
                                 {
                                     maxDate = financialPeriods.Rows[i].Field<DateTime>("StartDate");
                                     break;
                                 }
                             }
                                 throw new Exception("Resource is not available to work during the selected date range... Resource is  avialable from " + minDate.ToShortDateString() + " to " + maxDate.ToShortDateString());
                         }

                     }*/

                    //dsCopy.PlanResources.Load(planTable.CreateDataReader());
                    dsCopy.AcceptChanges();
                    for (int j = 0; j < dsCopy.Dates.Count; j++)
                    {

                        if (!dsCopy.PlanResources.Columns.Contains(dsCopy.Dates[j].IntervalName))
                        {
                            dsCopy.PlanResources.Columns.Add(dsCopy.Dates[j].IntervalName, typeof(Single));
                        }
                        double value = 0;
                        double totalValue = 0;
                        int noOfWorkingDays = 0;
                        for (DateTime k = dsCopy.Dates[j].StartDate; k < dsCopy.Dates[j].EndDate; k = k.AddDays(1))
                        {


                            if (IntervalDict.ContainsKey(k.Date))
                            {
                                if (workScale.ToUpper() == "HOURS")
                                {
                                    value = (ds.PlanResources[0][IntervalDict[k.Date]] != System.DBNull.Value ? Convert.ToSingle(ds.PlanResources[0][IntervalDict[k.Date]].ToString()) : 0.0);
                                    totalValue += value;
                                }
                                else
                                {
                                    if (!finanacialPeriodsTimePhasedData.ContainsKey(k.Date.ToShortDateString()))
                                    {
                                        throw new Exception("Resource is not available to work during the selected date range");
                                    }
                                    if (finanacialPeriodsTimePhasedData[k.Date.ToShortDateString()] > 0)
                                    {
                                        noOfWorkingDays++;
                                    }

                                    value = (ds.PlanResources[0][IntervalDict[k.Date]] != System.DBNull.Value ? Convert.ToSingle(ds.PlanResources[0][IntervalDict[k.Date]].ToString()) : 0.0);

                                    totalValue += value;

                                }
                            }
                        }
                        if (workScale.ToUpper() == "HOURS")
                        {
                            value = totalValue;
                        }
                        else
                        {
                            if (noOfWorkingDays
                                 > 0)
                            {
                                value = totalValue / noOfWorkingDays;
                            }
                        }

                        dsCopy.PlanResources[0][dsCopy.Dates[j].IntervalName] = value;

                    }

                    if (!isPSIVersion)
                    {
                        dsCopy.PlanResources.Columns.Add("ProjectName", typeof(string));
                        var projectList = ReadProjectsList();
                        Dictionary<Guid, string> projectMap = projectList.Project.AsEnumerable().ToDictionary(t => t.Field<Guid>("PROJ_UID"), t => t.Field<string>("PROJ_NAME"));
                        dsCopy.PlanResources[0]["ProjectName"] = projectMap[projectUiid];
                    }
                    if (workScale.ToUpper() == "HOURS")
                    {
                        ConvertHourScale(dsCopy);
                    }
                    //AddCheckoutFlags(projectUiid, dsCopy);
                    return dsCopy;
                }
                else
                {

                    var ds = PJPSIContext.ResourcePlanWebService.ReadResourcePlan(string.Empty, projectUiid
                        , sDate, eDate, intTimeScale, workScale.ToUpper() == "FTE", false);
                    if (!isPSIVersion)
                    {
                        ds.PlanResources.Columns.Add("ProjectName", typeof(string));
                        var projectList = ReadProjectsList();
                        Dictionary<Guid, string> projectMap = projectList.Project.AsEnumerable().ToDictionary(t => t.Field<Guid>("PROJ_UID"), t => t.Field<string>("PROJ_NAME"));
                        ds.PlanResources[0]["ProjectName"] = projectMap[projectUiid];
                    }

                    if (workScale.ToUpper() == "HOURS")
                    {
                        ConvertHourScale(ds);
                    }
                    //AddCheckoutFlags(projectUiid, ds);
                    return ds;
                }
            }
            catch (Exception ex1)
            {
                throw ex1;
            }

        }

        public ResPlan[] GetResourcePlan(DateTime sDate, DateTime eDate,
            string timeScale, string workScale, Guid projectUiid, string projectName, bool isPSIVersion, string isCheckedOut, string checkOutBY)
        {
            try
            {
                short intTimeScale = 5;
                switch (timeScale)
                {
                    case "Weeks":
                        intTimeScale = 4;
                        break;
                    case "Calendar Months":
                        intTimeScale = 5;
                        break;
                    case "Financial Months":
                        intTimeScale = 5;
                        break;
                    case "Years":
                        intTimeScale = 7;
                        break;
                }

                //TODO===============================
                //if (timeScale == "Financial Months")
                //{
                //===============================================
                //DataTable allPeriods = GetFinancialPeriods(sDate, eDate);

                //PJSchema.ResourcePlanDataSet.DatesDataTable financialPeriods = GetFinancialPeriods(allPeriods);
                //sDate = financialPeriods[0].StartDate;
                //eDate = financialPeriods[financialPeriods.Count - 1].EndDate;

                PJSchema.ResourcePlanDataSet ds = PJPSIContext.ResourcePlanWebService.ReadResourcePlan("",
                    projectUiid
                    , sDate, eDate, intTimeScale, workScale.ToUpper() == "FTE", false);
                if (workScale.ToUpper() == "HOURS")
                {
                    ConvertHourScale(ds);
                }
                if (workScale.ToUpper() == "DAYS")
                {
                    ConvertDaysScale(ds);
                }
                //TODo
                //PJSchema.ResourcePlanDataSet dsCopy = new PJSchema.ResourcePlanDataSet();
                // PJSchema.ResourcePlanDataSet.PlanResourcesDataTable planTable = dsCopy.PlanResources;

                //TODO var planRow = planTable.NewPlanResourcesRow();
                //dsCopy.Dates.Clear();
                //foreach (DataRow interval in financialPeriods.Rows)
                //{
                //    dsCopy.Dates.ImportRow(interval);
                //}
                //Dictionary<DateTime, string> IntervalDict = ds.Dates.AsEnumerable().ToDictionary(t => t.Field<DateTime>("StartDate").Date, t => t.Field<string>("IntervalName"));
                //AddCheckoutFlags(projectUiid, ds);
                List<ResPlan> resPlans = new List<ResPlan>();
                if (ds.PlanResources.Count < 1)
                {
                    ResPlan plan = new ResPlan();
                    plan.resource = new Resource() { resUid = Guid.Empty.ToString(), resName = "" };

                    plan.projects = new Project[1] { new Project() { projUid = projectUiid.ToString(), projName = projectName, readOnly = true } };
                    //plan.projects[0].readOnly = true;
                    //plan.projects[0].readOnlyReason = "Unable to retrieve data. Possible reason:Resource Plan requires publishing";
                    plan.projects[0].stalePublish = true;

                    resPlans.Add(plan);
                    return resPlans.ToArray();
                }
                for (int i = 0; i < ds.PlanResources.Count; i++)
                {
                    ResPlan plan = new ResPlan();
                    plan.resource = new Resource() { resUid = ds.PlanResources[i].RES_UID.ToString(), resName = ds.PlanResources[i].RES_NAME };

                    plan.projects = new Project[1] { new Project() { projUid = projectUiid.ToString(), projName = projectName, readOnly = false } };
                    plan.projects[0].intervals = new Intervals[ds.Dates.Count];
                    //if (ds.PlanResources[0]["IsCheckedOut"] != DBNull.Value && ds.PlanResources[0]["IsCheckedOut"].ToString() == "1")
                    //{
                    //    plan.projects[0].readOnly = true;
                    //    plan.projects[0].readOnlyReason = "Resource Plan is checked out by " + ds.PlanResources[0]["CheckoutBy"].ToString();
                    //}

                    for (int j = 0; j < ds.Dates.Count; j++)
                    {
                        plan.projects[0].intervals[j] = new Intervals();
                        plan.projects[0].intervals[j].intervalName = ds.Dates[j].IntervalName;
                        plan.projects[0].intervals[j].start = ds.Dates[j].StartDate.ToShortDateString();
                        plan.projects[0].intervals[j].end = ds.Dates[j].EndDate.ToShortDateString();
                        plan.projects[0].intervals[j].intervalValue = ds.PlanResources[i][ds.Dates[j].IntervalName].ToString();
                    }
                    resPlans.Add(plan);
                }





                //TODO
                //planRow.ASSN_BOOKING_TYPE = ds.PlanResources[0].ASSN_BOOKING_TYPE;
                //planTable.AddPlanResourcesRow(planRow);
                //Dictionary<string, decimal> finanacialPeriodsTimePhasedData = GetFinanacialTimePhasedData(resUID, sDate, eDate);
                //NOT TODO
                /* if(finanacialPeriodsTimePhasedData.Keys.Count < (eDate - sDate).Days)
                 {
                     finanacialPeriodsTimePhasedData = GetFinanacialTimePhasedData(resUID, null, null);
                     if(finanacialPeriodsTimePhasedData.Keys.Count < 1)
                     {
                         throw new Exception("Resource is not available to work. Please schedule the resource availabilty or contact the administrator");
                     }
                     else
                     {
                         DateTime minDate = finanacialPeriodsTimePhasedData.Keys.Min(t=>Convert.ToDateTime(t));
                         DateTime maxDate = finanacialPeriodsTimePhasedData.Keys.Max(t => Convert.ToDateTime(t));
                         for (int i=0;i<financialPeriods.Rows.Count;i++)
                         {
                             if (minDate == financialPeriods.Rows[i].Field<DateTime>("StartDate"))
                             {
                                 minDate = financialPeriods.Rows[i].Field<DateTime>("StartDate");
                                 break;
                             }
                             if(minDate > financialPeriods.Rows[i].Field<DateTime>("StartDate"))
                             {
                                 minDate = financialPeriods.Rows[i].Field<DateTime>("EndDate");
                                 break;
                             }
                         }

                         for (int i = financialPeriods.Rows.Count - 1; i >= 0; i--)
                         {
                             if (maxDate == financialPeriods.Rows[i].Field<DateTime>("EndDate"))
                             {
                                 maxDate = financialPeriods.Rows[i].Field<DateTime>("EndDate");
                                 break;
                             }

                             if (maxDate < financialPeriods.Rows[i].Field<DateTime>("EndDate"))
                             {
                                 maxDate = financialPeriods.Rows[i].Field<DateTime>("StartDate");
                                 break;
                             }
                         }
                             throw new Exception("Resource is not available to work during the selected date range... Resource is  avialable from " + minDate.ToShortDateString() + " to " + maxDate.ToShortDateString());
                     }

                 }*/

                //dsCopy.PlanResources.Load(planTable.CreateDataReader());

                //NOT TODO ENds
                //=========================================================
                //TODO BEINS
                //dsCopy.AcceptChanges();
                //for (int j = 0; j < dsCopy.Dates.Count; j++)
                //{

                //    if (!dsCopy.PlanResources.Columns.Contains(dsCopy.Dates[j].IntervalName))
                //    {
                //        dsCopy.PlanResources.Columns.Add(dsCopy.Dates[j].IntervalName, typeof(Single));
                //    }
                //    double value = 0;
                //    double totalValue = 0;
                //    int noOfWorkingDays = 0;
                //    for (DateTime k = dsCopy.Dates[j].StartDate; k < dsCopy.Dates[j].EndDate; k = k.AddDays(1))
                //    {


                //        if (IntervalDict.ContainsKey(k.Date))
                //        {
                //            if (workScale.ToUpper() == "HOURS")
                //            {
                //                value = (ds.PlanResources[0][IntervalDict[k.Date]] != System.DBNull.Value ? Convert.ToSingle(ds.PlanResources[0][IntervalDict[k.Date]].ToString()) : 0.0);
                //                totalValue += value;
                //            }
                //            else
                //            {
                //                if (!finanacialPeriodsTimePhasedData.ContainsKey(k.Date.ToShortDateString()))
                //                {
                //                    throw new Exception("Resource is not available to work during the selected date range");
                //                }
                //                if (finanacialPeriodsTimePhasedData[k.Date.ToShortDateString()] > 0)
                //                {
                //                    noOfWorkingDays++;
                //                }

                //                value = (ds.PlanResources[0][IntervalDict[k.Date]] != System.DBNull.Value ? Convert.ToSingle(ds.PlanResources[0][IntervalDict[k.Date]].ToString()) : 0.0);

                //                totalValue += value;

                //            }
                //        }
                //    }
                //    if (workScale.ToUpper() == "HOURS")
                //    {
                //        value = totalValue;
                //    }
                //    else
                //    {
                //        if (noOfWorkingDays
                //             > 0)
                //        {
                //            value = totalValue / noOfWorkingDays;
                //        }
                //    }

                //    dsCopy.PlanResources[0][dsCopy.Dates[j].IntervalName] = value;

                //}

                //if (!isPSIVersion)
                //{
                //    dsCopy.PlanResources.Columns.Add("ProjectName", typeof(string));
                //    var projectList = ReadProjectsList();
                //    Dictionary<Guid, string> projectMap = projectList.Project.AsEnumerable().ToDictionary(t => t.Field<Guid>("PROJ_UID"), t => t.Field<string>("PROJ_NAME"));
                //    dsCopy.PlanResources[0]["ProjectName"] = projectMap[projectUiid];
                //}
                //if (workScale.ToUpper() == "HOURS")
                //{
                //    ConvertHourScale(dsCopy);
                //}
                //AddCheckoutFlags(projectUiid, dsCopy,isCheckedOut,checkOutBY);
                //return dsCopy;
                //}
                //else
                //{

                //    var ds = PJPSIContext.ResourcePlanWebService.ReadResourcePlan(filter.GetXml(), projectUiid
                //        , sDate, eDate, intTimeScale, workScale.ToUpper() != "HOURS", false);
                //    if (!isPSIVersion)
                //    {
                //        ds.PlanResources.Columns.Add("ProjectName", typeof(string));
                //        var projectList = ReadProjectsList();
                //        Dictionary<Guid, string> projectMap = projectList.Project.AsEnumerable().ToDictionary(t => t.Field<Guid>("PROJ_UID"), t => t.Field<string>("PROJ_NAME"));
                //        ds.PlanResources[0]["ProjectName"] = projectMap[projectUiid];
                //    }

                //    if (workScale.ToUpper() == "HOURS")
                //    {
                //        ConvertHourScale(ds);
                //    }
                //    AddCheckoutFlags(projectUiid, ds,isCheckedOut,checkOutBY);
                //    return ds;
                //}
                return resPlans.ToArray();
            }
            catch (Exception ex1)
            {
                throw ex1;
            }

        }

        private void AddCheckoutFlags(Guid projectUiid, PJSchema.ResourcePlanDataSet dsCopy)
        {
            var checkedOutInfo = GetCheckedOutInfo(projectUiid);
            dsCopy.PlanResources.Columns.Add("IsCheckedOut", typeof(string));
            dsCopy.PlanResources.Columns.Add("CheckoutBy", typeof(string));
            dsCopy.PlanResources.Columns.Add("IsStalePublish", typeof(string));
            //bool isStalePublish = PJPSIContext.PWAWebService.ProjectGetProjectIsPublished(projectUiid) == 1;
            //dsCopy.PlanResources[0]["IsStalePublish"] = (!isStalePublish).ToString();
            if (dsCopy.PlanResources.Count > 0)
            {
                dsCopy.PlanResources[0]["IsCheckedOut"] = string.IsNullOrWhiteSpace(checkedOutInfo.User) ? "0" : "1";
                dsCopy.PlanResources[0]["CheckoutBy"] = checkedOutInfo.User;
            }
        }

        public bool IsprojectStalePublish(Guid projuid)
        {
            return !(PJPSIContext.PWAWebService.ProjectGetProjectIsPublished(projuid) == 1);
        }

        private static void ConvertHourScale(PJSchema.ResourcePlanDataSet ds)
        {
            for (int i = 0; i < ds.PlanResources.Count; i++)
            {
                foreach (PJSchema.ResourcePlanDataSet.DatesRow dateRow in ds.Dates)
                {
                    ds.PlanResources[i][dateRow.IntervalName] = ds.PlanResources[i][dateRow.IntervalName] != System.DBNull.Value ? Convert.ToDouble(ds.PlanResources[i][dateRow.IntervalName]) * 8 / 4800 : 0.0;
                }
            }
        }
        private static void ConvertDaysScale(PJSchema.ResourcePlanDataSet ds)
        {
            for (int i = 0; i < ds.PlanResources.Count; i++)
            {
                foreach (PJSchema.ResourcePlanDataSet.DatesRow dateRow in ds.Dates)
                {
                    ds.PlanResources[i][dateRow.IntervalName] = ds.PlanResources[i][dateRow.IntervalName] != System.DBNull.Value ? Convert.ToDouble(ds.PlanResources[i][dateRow.IntervalName]) / 4800 : 0.0;
                }
            }
        }

        public bool CheckoutProjectPlan(string projectUID)
        {
            try
            {
                PJPSIContext.ResourcePlanWebService.CheckOutResourcePlans(new Guid[1] { new Guid(projectUID) });
            }
            catch
            {
                return false;
            }
            return true;
        }



        public UpdateResult UpdateResourcePlan(PJSchema.ResourcePlanDataSet dataSet, bool isNew, string projUid, string timescale, string workScale)
        {
            try
            {


                UpdateResult result = new UpdateResult() { project = new Project() { projUid = projUid } };
                if (isNew)
                {
                    return CreateResourcePlan(dataSet, projUid, workScale);
                }
                Guid updateAndCheckinJobUid = Guid.NewGuid();
                bool res;

                //if stale publish

                if (IsprojectStalePublish(new Guid(projUid)))
                {
                    var updateResult = PublishProject(projUid);
                    if (updateResult.success == false)
                    {
                        return updateResult;
                    }
                    //CheckoutProject(projUid);
                }

                CheckoutResourcePlan(projUid);

                //PrepareResourcePlanDataSet(dataSet, timescale.ToUpper() == "FINANCIAL MONTHS" && workScale.ToString().ToUpper() == "FTE");

                lock (PJPSIContext)
                {
                    PJPSIContext.ResourcePlanWebService.QueueUpdateResourcePlan(new Guid(projUid), dataSet, workScale.ToUpper() == "FTE", false, updateAndCheckinJobUid);



                    res = QueueHelper.WaitForQueueJobCompletion(updateAndCheckinJobUid,
                       (int)PSLib.QueueConstants.QueueMsgType.ResourcePlanSave, PJPSIContext);

                    if (!res)
                    {
                        result.success = false;
                        result.debugError = "Wait for Queue failed for Save Resource Plan";
                        result.error = "An unexpected error occured in Saving resource plan";
                        return result;
                    }

                    return PublishResourcePlan(dataSet, projUid, updateAndCheckinJobUid);
                }
            }
            catch (Exception ex)
            {
                UpdateResult result;
                ExceptionUtility.HandleException(ex, projUid, "", out result);
                return result;
            }

        }

        private UpdateResult PublishProject(string projUid)
        {
            try
            {
                var result = new UpdateResult() { project = new Project() { projUid = projUid } };
                var sessionGuid = Guid.NewGuid();
                var jobGuid = Guid.NewGuid();
                
                PJPSIContext.ProjectWebService.CheckOutProject(new Guid(projUid), sessionGuid, "");
                PJPSIContext.ProjectWebService.QueuePublish(jobGuid, new Guid(projUid), true, null);
                var res = QueueHelper.WaitForQueueJobCompletion(jobGuid,
                       (int)PSLib.QueueConstants.QueueMsgType.ResourcePlanSave, PJPSIContext);

                if (!res)
                {
                    result.success = false;
                    result.debugError = "Wait for Queue failed for Publish project(Stale Publish)";
                    result.error = "An unexpected error occured in Publishing project for the resource plan";
                    return result;
                }
                result.success = true;
                return result;
            }
            catch (Exception ex)
            {
                UpdateResult result;
                ExceptionUtility.HandleException(ex, projUid, "", out result);
                return result;
            }
        }

        private UpdateResult CreateResourcePlan(PJSchema.ResourcePlanDataSet dataSet, string projUid, string workScale)
        {
            var result = new UpdateResult() { project = new Project() { projUid = projUid } };
            try
            {
                Guid CreateAndCheckinJobGuid = Guid.NewGuid();
                CreateResourcePlan(dataSet, workScale.ToUpper() == "FTE", CreateAndCheckinJobGuid);
                CheckoutResourcePlan(projUid);
                PublishResourcePlan(dataSet, projUid, CreateAndCheckinJobGuid);
                result.success = true;
                return result;
            }
            catch (Exception ex)
            {
                ExceptionUtility.HandleException(ex, projUid, "", out result);
                return result;
            }
        }

        private UpdateResult CreateResourcePlan(PJSchema.ResourcePlanDataSet dataSet, bool isFTE,Guid CreateAndCheckinJobGuid)
        {
            var result = new UpdateResult() { project = new Project() { projUid = dataSet.PlanResources[0].PROJ_UID.ToString() } };
            //var jobGuid = Guid.NewGuid();
            try
            {
                PJPSIContext.ResourcePlanWebService.QueueCreateResourcePlan(dataSet.PlanResources[0].PROJ_UID, dataSet, isFTE, true, CreateAndCheckinJobGuid);

                var res = QueueHelper.WaitForQueueJobCompletion(CreateAndCheckinJobGuid,
                  (int)PSLib.QueueConstants.QueueMsgType.ResourcePlanCheckIn, PJPSIContext);
                if (!res)
                {
                    result.success = false;
                    result.debugError = "Wait for Queue failed for Create Resource Plan";
                    result.error = "An unexpected error occured in Creating Resource Plann";
                    return result;
                }
                result.success = true;
                return result;
            }
            catch (Exception ex)
            {
                result.debugError = ex.Message;
                result.error = "An unexpected error occured in Creating Resource Plan";
                return result;
            }
        }

        private void CheckoutResourcePlan(string projectUid)
        {

            PJPSIContext.ResourcePlanWebService.CheckOutResourcePlans(new Guid[1] { new Guid(projectUid) });

        }

        private void PrepareResourcePlanDataSet(PJSchema.ResourcePlanDataSet dataSet, bool isFinanacialMonthsAndFTE)
        {
            dataSet.PlanResources.PrimaryKey = new DataColumn[] { dataSet.PlanResources.Columns["RES_UID"], dataSet.PlanResources.Columns["PROJ_UID"] };
            for (int i = dataSet.PlanResources.Columns.Count - 1; i >= 0; i--)
            {
                DataColumn column = dataSet.PlanResources.Columns[i];
                if ((dataSet.PlanResources.Columns.CanRemove(column) && !column.ColumnName.StartsWith("Interval", StringComparison.OrdinalIgnoreCase)) && (!column.ColumnName.Equals("ASSN_UID") && !column.ColumnName.Equals("ASSN_BOOKING_TYPE")))
                {
                    dataSet.PlanResources.Columns.Remove(column);
                }
            }
            if (isFinanacialMonthsAndFTE)
            {
                for (int i = 0; i < dataSet.Dates.Count; i++)
                {
                    dataSet.Dates[i].EndDate = dataSet.Dates[i].EndDate.AddDays(1);
                }
            }
            //dataSet.Dates.AcceptChanges();
        }


        public UpdateResult PublishResourcePlan(PJSchema.ResourcePlanDataSet dataSet, string projUid, Guid updateAndCheckinJobUid)
        {
            Guid jobUid = Guid.NewGuid();
            var result = new UpdateResult() { project = new Project() };
            Guid sessionUID = Guid.NewGuid();

            PJPSIContext.ResourcePlanWebService.QueuePublishResourcePlan(new Guid(projUid), jobUid);
            result.project.projUid = projUid;


            bool res = QueueHelper.WaitForQueueJobCompletion(jobUid,
                (int)PSLib.QueueConstants.QueueMsgType.ResourcePlanPublish, PJPSIContext);


            if (!res)
            {
                result.success = false;
                result.debugError = "Wait for Queue failed for Pubish Resource Plan";
                result.error = "An unexpected error occured in Publishing resource plan";
                return result;
            }
            jobUid = Guid.NewGuid();

            PJPSIContext.ResourcePlanWebService.QueueCheckInResourcePlans(new Guid[1] { new Guid(projUid) }, true, new Guid[1] { updateAndCheckinJobUid });

            res = QueueHelper.WaitForQueueJobCompletion(updateAndCheckinJobUid,
               (int)PSLib.QueueConstants.QueueMsgType.ResourcePlanCheckIn, PJPSIContext);
            if (!res)
            {
                result.success = false;
                result.debugError = "Wait for Queue failed for Check in Resource Plan";
                result.error = "An unexpected error occured in Checking in resource plan";
                return result;
            }
            result.success = true;
            return result;
        }


        internal List<Dictionary<string, object>> GetProjects(List<string> projectswithPlans, List<string> columns)
        {
            var allProjects = ReadProjectsList();

            PJSchema.ProjectDataSet projectsToAdd = new PJSchema.ProjectDataSet();
            if (projectswithPlans.Count > 0)
            {
                foreach (DataRow row in allProjects.Project.Rows)
                {
                    if (!projectswithPlans.Any(t => t == row.Field<Guid>("PROJ_UID").ToString()))
                    {
                        projectsToAdd.Project.ImportRow(row);
                    }
                }
            }
            else
            {
                projectsToAdd = allProjects;
            }
            var dt = GetProjectsData(projectsToAdd.Project.AsEnumerable().Select(t => t.Field<Guid>("PROJ_UID").ToString()).ToList(), columns);
            return null;
        }

        public virtual DataTable GetProjectsData(List<string> projects, List<string> columns)
        {
            string connectionString = ConfigurationUtility.GetConnectionString("DataMart");
            DataTable dt = new DataTable();
            try
            {
                string[] columnsCopy = new string[0];

                columnsCopy = columns.ToArray();

                dt = GetProjectsCustomFields(projects,
              columnsCopy, connectionString);

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {

            }
            return dt;
        }

        DataTable GetProjectsCustomFields(List<string> puids, string[] columns, string connectionString)
        {
            DataTable csTable = new DataTable();
            string puidsParam = string.Join(";", puids.ToArray());
            string columnsParam = string.Join(";", columns);
            SqlConnection sqlClient = new SqlConnection(connectionString);
            SqlDataAdapter sa = new SqlDataAdapter();
            SqlCommand sq = new SqlCommand();
            sq.Connection = sqlClient;
            sq.CommandText = "usp_GetProjectData_RP";
            sq.CommandType = CommandType.StoredProcedure;
            sq.Parameters.Add("@ProjectUID", SqlDbType.NVarChar);
            sq.Parameters["@ProjectUID"].Value = puidsParam;
            sq.Parameters.Add("@MDPropUID", SqlDbType.NVarChar);
            sq.Parameters["@MDPropUID"].Value = columnsParam.Replace("PROJ_UID", "").Replace("ProjectName;", "").Replace("Owner;", "").Replace("Owner", "").Trim(';');
            sa.SelectCommand = sq;
            sa.Fill(csTable);
            return csTable;

        }

        internal DataTable GetFinancialPeriods(DateTime startDate, DateTime endDate)
        {
            lock (HttpContext.Current.Application)
            {
                DataTable dt = new DataTable();
                SqlConnection connection = new SqlConnection(ConfigurationUtility.GetConnectionString("DataMart"));
                SqlCommand command = new SqlCommand();
                command.Connection = connection;
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = 60;
                command.CommandText = "usp_GetFinancialMonths";
                command.Parameters.Add("@StartDate", SqlDbType.DateTime);
                command.Parameters["@StartDate"].Value = startDate;
                command.Parameters.Add("@EndDate", SqlDbType.DateTime);
                command.Parameters["@EndDate"].Value = endDate;
                SqlDataAdapter adapter = new SqlDataAdapter();
                adapter.SelectCommand = command;
                adapter.Fill(dt);
                return dt;
            }
        }

        internal PJSchema.ResourcePlanDataSet.DatesDataTable GetFinancialPeriods(DataTable dt)
        {
            PJSchema.ResourcePlanDataSet.DatesDataTable datesTable = new PJSchema.ResourcePlanDataSet.DatesDataTable();
            int i = 0;
            foreach (DataRow row in dt.Rows)
            {

                var datesRow = datesTable.NewDatesRow();
                datesRow.IntervalName = "Interval" + i.ToString();
                datesRow.StartDate = Convert.ToDateTime(row["StartDate"].ToString()).Date;
                datesRow.EndDate = Convert.ToDateTime(row["EndDate"].ToString()).Date;
                datesTable.AddDatesRow(datesRow);
                i++;
            }
            return datesTable;
        }

        internal UpdateResult AddResourcePlan(string projectUID, string projectName, string resourceUID, string timeScale, string workScale, string startDate, string endDate)
        {
            Guid jobUid = Guid.NewGuid();
            var project = new Project();
            project.projUid = projectUID;

            UpdateResult res = new UpdateResult() { project = project };
            /*
                        var team = PJPSIContext.ProjectWebService.ReadProjectTeam(new Guid(projectUID));

                        if (!team.ProjectTeam.AsEnumerable().Any(t => t.Field<Guid>("RES_UID") == new Guid(resourceUID)))
                        {

                            res = AddTeamMember(projectUID, resourceUID, team, out res);
                            if (!res) return res;
                        }

                        */
            try
            {
                short intTimeScale = 5;
                switch (timeScale)
                {
                    case "Weeks":
                        intTimeScale = 4;
                        break;
                    case "Calendar Months":
                        intTimeScale = 5;
                        break;
                    case "Financial Months":
                        intTimeScale = 5;
                        break;
                    case "Years":
                        intTimeScale = 7;
                        break;
                }
                lock (PJPSIContext)
                {
                    // Get Resource data set for project
                    PJSchema.ResourcePlanDataSet rds = new PJSchema.ResourcePlanDataSet();
                    Microsoft.Office.Project.Server.Library.Filter filter = new Microsoft.Office.Project.Server.Library.Filter();
                    filter.FilterTableName = rds.PlanResources.TableName;
                    Microsoft.Office.Project.Server.Library.Filter.FieldOperator op =
                        new Microsoft.Office.Project.Server.Library.Filter.FieldOperator(Microsoft.Office.Project.Server.Library.Filter.FieldOperationType.Equal,
                       rds.PlanResources.RES_UIDColumn.ColumnName, new Guid(resourceUID));
                    filter.Criteria = op;
                    var resPlanCopy = PJPSIContext.ResourcePlanWebService.ReadResourcePlan(filter.GetXml(), new Guid(projectUID), Convert.ToDateTime(startDate),
                        Convert.ToDateTime(endDate), intTimeScale, workScale.ToUpper() == "FTE", false);
                    PJSchema.ResourcePlanDataSet resPlans = new PJSchema.ResourcePlanDataSet();

                    if (!(resPlanCopy.PlanResources.AsEnumerable().Any(t => t.Field<Guid>("PROJ_UID") == new Guid(projectUID) && t.Field<Guid>("RES_UID") == new Guid(resourceUID))))
                    {
                        var newRow = resPlanCopy.PlanResources.NewPlanResourcesRow();
                        newRow.PROJ_UID = new Guid(projectUID);
                        newRow.RES_UID = new Guid(resourceUID);
                        newRow.ASSN_BOOKING_TYPE = (byte)Microsoft.Office.Project.Server.Library.Resource.BookingType.Committed;

                        resPlanCopy.PlanResources.AddPlanResourcesRow(newRow);

                    }

                    resPlans = resPlanCopy;


                    // Check if Res plan exists at all
                    var isNew = PJPSIContext.ResourcePlanWebService.ReadResourcePlanStatus(new Guid(projectUID)).Equals(PSLib.ResourcePlan.ResPlanStatus.Absent);

                    //if(isNew)
                    //{
                    //    var row = resPlans.PlanResources.NewPlanResourcesRow();
                    //    row.PROJ_UID = new Guid(projectUID);
                    //    row.RES_UID = new Guid(resourceUID);
                    //    row.ASSN_BOOKING_TYPE = (byte)Microsoft.Office.Project.Server.Library.Resource.BookingType.Committed;
                    //    resPlans.PlanResources.AddPlanResourcesRow(row);
                    //}

                    foreach (PJSchema.ResourcePlanDataSet.DatesRow interval in resPlans.Dates)
                    {
                        resPlans.PlanResources[0][interval.IntervalName] = 0;
                    }
                    res = UpdateResourcePlan(resPlans, isNew, projectUID, timeScale, workScale);
                    var resPlanDs = PJPSIContext.ResourcePlanWebService.ReadResourcePlan(filter.GetXml(), new Guid(projectUID), Convert.ToDateTime(startDate),
                        Convert.ToDateTime(endDate), intTimeScale, workScale.ToUpper() == "FTE", false);
                    project.intervals = new Intervals[resPlans.Dates.Count];
                    var counter = 0;
                    foreach (PJSchema.ResourcePlanDataSet.DatesRow interval in resPlans.Dates)
                    {
                        project.intervals[counter++] = new Intervals() { start = interval.StartDate.ToShortDateString(), end = interval.EndDate.ToShortDateString(), intervalName = interval.IntervalName, intervalValue = resPlans.PlanResources[0][interval.IntervalName].ToString() };
                    }
                    res.project = project;
                    res.project.projName = projectName;
                    res.success = true;
                    return res;
                }
            }
            catch (Exception ex)
            {
                ExceptionUtility.HandleException(ex, projectUID, projectName, out res);
                return res;
            }
        }

        internal UpdateResult DeleteResourcePlan(UpdateResPlan resPlan, string timeScale, string workScale, string startDate, string endDate)
        {
            var result = new UpdateResult() { project = new Project() { projUid = resPlan.Project.projUid, projName = resPlan.Project.projName } };
            try
            {
                Guid jobUid = Guid.NewGuid();

                short intTimeScale = 5;
                switch (timeScale)
                {
                    case "Weeks":
                        intTimeScale = 4;
                        break;
                    case "Calendar Months":
                        intTimeScale = 5;
                        break;
                    case "Financial Months":
                        intTimeScale = 5;
                        break;
                    case "Years":
                        intTimeScale = 7;
                        break;
                }
                // Get Resource data set for project
                var resPlanCopy = PJPSIContext.ResourcePlanWebService.ReadResourcePlan(null, new Guid(resPlan.Project.projUid), Convert.ToDateTime(startDate),
                    Convert.ToDateTime(endDate), intTimeScale, workScale.ToUpper() == "FTE", false);

                var resPlanRows = resPlanCopy.PlanResources.AsEnumerable().Where(t => resPlan.Project.resources.Any(r => new Guid(r.resource.resUid) == t.Field<Guid>("RES_UID")
                && t.Field<Guid>("PROJ_UID") == new Guid(resPlan.Project.projUid)));
                foreach (var resPlanRow in resPlanRows)
                {
                    resPlanRow.Delete();
                }


                return UpdateResourcePlan(resPlanCopy, false, resPlan.Project.projUid, timeScale, workScale);

            }
            catch (Exception ex)
            {

                ExceptionUtility.HandleException(ex, resPlan.Project.projUid, resPlan.Project.projName, out result);
                return result;
            }
        }

        internal DataTable GetProjectsWithResourcePlansForResource(string ruid)
        {
            //usp_GetPuidsForResourceWithResourcePlans
            DataTable csTable = new DataTable();
            //string puidsParam = string.Join(";", pUIDS.ToArray());
            SqlConnection sqlClient = new SqlConnection(ConfigurationUtility.GetConnectionString("DataMart"));
            SqlDataAdapter sa = new SqlDataAdapter();
            SqlCommand sq = new SqlCommand();
            sq.Connection = sqlClient;
            sq.CommandText = "usp_GetPuidsForResourceWithResourcePlans";
            sq.CommandType = CommandType.StoredProcedure;
            sq.Parameters.Add("@ResourceUID", SqlDbType.UniqueIdentifier);
            sq.Parameters["@ResourceUID"].Value = new Guid(ruid);
            sa.SelectCommand = sq;
            sa.Fill(csTable);
            return csTable;
        }

        public CheckedOutInfo GetCheckedOutInfo(Guid projUID)
        {
            //CheckedOutInfo info = new CheckedOutInfo() { User = "", PROJ_NAME = "" };
            //var checkedOutPlans = PJPSIContext.PWAWebService.AdminReadCheckedOutEnterpriseResourcePlans();
            //foreach(PJSchema.AdminCheckedOutResourcePlansDataSet.CheckedOutResourcePlansRow row in checkedOutPlans.CheckedOutResourcePlans)
            //{
            //    if(row.PROJ_UID == projUID)
            //    {
            //        info.PROJ_NAME = row.PROJ_NAME;
            //        info.User = row.RES_NAME;
            //        break;
            //    }
            //}
            return new CheckedOutInfo();
        }
    }


}
