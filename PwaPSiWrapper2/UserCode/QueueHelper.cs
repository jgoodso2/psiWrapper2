using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PSLib = Microsoft.Office.Project.Server.Library;
using PJSchema = Microsoft.Office.Project.Server.Schema;
using PSPJLib = Microsoft.Office.Project.PWA;
using System.Security.Principal;

using System.Data;

using System.Xml;
using System.Data.SqlClient;
using System.Configuration;
using Microsoft.SharePoint;
using System.Web;
using Microsoft.Office.Project.PWA;
using Microsoft.Office.Project.Server.Schema;
using Microsoft.Office.Project.Server.Library;

namespace PwaPSIWrapper.UserCode.PwaGatewayCommands
{
    public static class QueueHelper
    {
        private static List<int> CheckStatusRowErrors(string errorInfo)
        {
            List<int> errorList = new List<int>();
            bool containsError = false;

            XmlTextReader xReader = new XmlTextReader(new System.IO.StringReader(errorInfo));
            while (xReader.Read())
            {
                if (xReader.Name == "errinfo" && xReader.NodeType == XmlNodeType.Element)
                {
                    xReader.Read();
                    if (xReader.Value != string.Empty)
                    {
                        containsError = true;
                    }
                }
                if (containsError && xReader.Name == "error" && xReader.NodeType == XmlNodeType.Element)
                {
                    while (xReader.Read())
                    {
                        if (xReader.Name == "id" && xReader.NodeType == XmlNodeType.Attribute)
                        {
                            errorList.Add(Convert.ToInt32(xReader.Value));
                        }
                    }
                }
            }
            return errorList;
        }
        public static bool WaitForQueueJobCompletion(Guid jobGuid, int messageType,PSPJLib.PSI pjContext)
        {
           

            //lock (locker)
            //{
            Microsoft.Office.Project.Server.Schema.QueueStatusDataSet queueStatusDataSet = new Microsoft.Office.Project.Server.Schema.QueueStatusDataSet();

            //try this
            //  Microsoft.Office.Project.Server.Schema.QueueStatusRequestDataSet queueStatusRequestDataSet = new Microsoft.Office.Project.Server.Schema.QueueStatusRequestDataSet();



            bool inProcess = true;
            bool result = false;
            DateTime startTime = DateTime.Now;
            int successState = (int)Microsoft.Office.Project.Server.Library.QueueConstants.JobState.Success;
            int failedState = (int)Microsoft.Office.Project.Server.Library.QueueConstants.JobState.Failed;
            int blockedState = (int)Microsoft.Office.Project.Server.Library.QueueConstants.JobState.CorrelationBlocked;

            List<int> errorList = new List<int>();
          
                try
                {

                    while (inProcess)
                    {
                        DateTime now = DateTime.Now;
                        DateTime toDate = now.AddMinutes(10);   //DateTime.Now + new TimeSpan(0, 0, 0, 0);
                        DateTime fromDate = now.AddHours(-3);


                        //queueStatusDataSet = PJWebPartContext.PSI.QueueSystemWebService.ReadJobStatus(queueStatusRequestDataSet, false,
                        //PSLib.QueueConstants.SortColumn.Undefined, PSLib.QueueConstants.SortOrder.Undefined);

                        QueueConstants.QueueMsgType[] messageTypes = new QueueConstants.QueueMsgType[] { (QueueConstants.QueueMsgType)Enum.Parse(typeof(QueueConstants.QueueMsgType), messageType.ToString()) };
                        // QueueConstants.JobState[] jobCompletionStates = new QueueConstants.JobState[] { QueueConstants.JobState.Success, QueueConstants.JobState.Failed, QueueConstants.JobState.Processing,QueueConstants.JobState.CorrelationBlocked };
                        //var jobStates = (QueueConstants.JobState[])Enum.GetValues(typeof(QueueConstants.JobState));
                        QueueConstants.JobState[] jobStates = new QueueConstants.JobState[]
    {
        QueueConstants.JobState.Success,
        QueueConstants.JobState.Canceled,
        QueueConstants.JobState.Failed,
        QueueConstants.JobState.Processing,

    };
                        // Guid[] projectGUIDs = new Guid[] { projGUID };
                        // new method to read queue status here
                        //queueStatusDataSet = PJWebPartContext.PSI.PWAWebService.QueueSystemReadMyJobStatusUI(messageTypes, jobCompletionStates, fromDate, toDate, -1, false,"","", QueueConstants.SortColumn.QueueEntryTime, QueueConstants.SortOrder.Descending);
                        //queueStatusDataSet = PJWebPartContext.PSI.PWAWebService.QueueSystemReadMyJobStatusUI(messageTypes, jobCompletionStates, fromDate, toDate, 150, false, "", "", QueueConstants.SortColumn.QueueEntryTime, QueueConstants.SortOrder.Descending);

                        //return base.PjContext.PSI.PWAWebService.QueueSystemReadMyJobStatusUI(null, jobCompletionState, fromDate, DateTime.MaxValue, 500, false, "", "", sortColumn, sortOrder);
                        //queueStatusDataSet = pjContext.QueueSystemWebService.ReadMyJobStatus(null,jobStates,fromDate, toDate,500,false, QueueConstants.SortColumn.QueueEntryTime, QueueConstants.SortOrder.Descending);
                        queueStatusDataSet = pjContext.PWAWebService.QueueSystemReadMyJobStatusUI(null, jobStates, fromDate, toDate, 500, false, "", "", QueueConstants.SortColumn.QueueEntryTime, QueueConstants.SortOrder.Descending);

                        //verify that rows are dissappearing... we had always assumed that they were not.
                        var queueCount = queueStatusDataSet.Status.Rows.Count;


                        foreach (Microsoft.Office.Project.Server.Schema.QueueStatusDataSet.StatusRow statusRow in queueStatusDataSet.Status)
                        {
                            //if (statusRow["ErrorInfo"] != System.DBNull.Value && statusRow.JobGUID == jobGuid)
                            {
                                errorList = CheckStatusRowErrors(statusRow["ErrorInfo"].ToString());

                                if (errorList.Count > 0
                                    || statusRow.JobCompletionState == blockedState
                                    || statusRow.JobCompletionState == failedState)
                                {
                                    inProcess = false;
                                    return false;
                                    //ShowErrorList("Queue", statusRow.JobCompletionState, errorList);
                                }
                            }
                            if (statusRow.JobCompletionState == successState && statusRow.JobGUID == jobGuid)
                            {


                                inProcess = false;
                                result = true;
                                return true;
                            }

                        }
                        DateTime endTime = DateTime.Now;
                        TimeSpan span = endTime.Subtract(startTime);


                        if (span.Seconds > 30) //Wait for only 20 secs - and then bail out.
                        {
                            Console.Write("Queue busy.  Please wait.");
                            inProcess = false;
                            result = false;
                            return false;
                        }
                        else
                        {
                            inProcess = true;
                            System.Threading.Thread.Sleep(500);  // Sleep 1/2 second.
                        }
                    }
                }
                catch (Exception ex)
                {
                }
                finally
                {


                }

            
            return result;
            //}
        }
    }
}
