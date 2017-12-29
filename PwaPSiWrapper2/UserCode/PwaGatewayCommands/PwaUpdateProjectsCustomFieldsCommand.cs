using Microsoft.Office.Project.PWA;
using PwaPSIWrapper.UserCode.PwaGatewayCommands.Entity;
using PwaPSIWrapper.UserCode.PwaGatewayCommands.Entity.Pwa;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;
using PSLib = Microsoft.Office.Project.Server.Library;

namespace PwaPSIWrapper.UserCode.PwaGatewayCommands
{
    public class PwaUpdateProjectsCustomFieldsCommand : IPwaCommand, IPwaCommandFactory, IPwaOutput
    {
        public PJContext _pj;

        public string PwaCommandDescription
        {
            get { return "PwaUpdateProjectsCustomFieldsCommand - needs a ProjectUID"; }
        }

        public string PwaCommandName
        {
            get { return "PwaUpdateProjectsCustomFieldsCommand"; }
            set { PwaCommandName = value; }
        }

        public string Output
        {
            get;
            set;
        }

        public UpdateResult Result
        {
            get; set;
        }
        public PwaUpdateProjectsCustomFieldsInput PwaInput;

        public void Execute()
        {
            try
            {
                var proj = PwaInput.Projects;
                DataTable dt = new DataTable();
                var row = dt.NewRow();
                foreach (KeyValuePair<string, object> item in proj)
                {
                    dt.Columns.Add(item.Key, typeof(string));
                    row[item.Key] = item.Value;
                }
                dt.Rows.Add(row);
                Result = UpdateProject(row,
                     PwaInput.Columns);
            }
            catch (Exception ex0)
            {
                UpdateResult upresult = new UpdateResult();
                upresult.error = "Error=" + ex0.Message;
            }
        }

        public IPwaCommand MakePwaCommand(PJContext pj, NameValueCollection args)
        {

            return new PwaUpdateProjectsCustomFieldsCommand() { _pj = pj, PwaInput =(PwaUpdateProjectsCustomFieldsInput) new PwaUpdateProjectsCustomFieldsInput(args).ParseInput() };
        }

        public UpdateResult UpdateProject(DataRow project, List<Settings> columns)
        {
            UpdateResult updateResult = new UpdateResult();
            try
            {

                var customFields = GetCustomFields(columns.Select(t => t.Name).ToList());
                updateResult.project.projName = project["ProjectName"].ToString();

                if (project["isDirty"] == null || project["isDirty"] == System.DBNull.Value || Convert.ToBoolean(project["isDirty"]) == false)
                {
                    updateResult.success = true;
                    return updateResult;
                }


                var projectdS = _pj.PSI.ProjectWebService.ReadProject(new Guid(project.Field<string>("PROJ_UID")), PSLib.DataStoreEnum.PublishedStore);
                foreach (var column1 in columns)
                {
                    var column = column1.Name.Trim();
                    if (!customFields.CustomFields.AsEnumerable().Any(p => p.Field<string>("MD_PROP_NAME") == column))
                    {
                        continue;
                    }
                    if (project[column] == null || project[column] == System.DBNull.Value)
                    {
                        continue;
                    }
                    Microsoft.Office.Project.Server.Schema.ProjectDataSet.ProjectCustomFieldsRow csRwo;
                    if (projectdS.ProjectCustomFields.AsEnumerable().Any(t => t.Field<Guid>("MD_PROP_UID") == customFields.CustomFields.
                        AsEnumerable().First(p => p.Field<string>("MD_PROP_NAME") == column).Field<Guid>("MD_PROP_UID")))
                    {
                        csRwo = projectdS.ProjectCustomFields.AsEnumerable().First(t => t.Field<Guid>("MD_PROP_UID") == customFields.CustomFields.AsEnumerable().First(p => p.
                            Field<string>("MD_PROP_NAME") == column).Field<Guid>("MD_PROP_UID")) as Microsoft.Office.Project.Server.Schema.ProjectDataSet.ProjectCustomFieldsRow;
                    }
                    else
                    {
                        csRwo = projectdS.ProjectCustomFields.NewProjectCustomFieldsRow();
                        csRwo.CUSTOM_FIELD_UID = Guid.NewGuid();
                        csRwo.MD_PROP_UID = customFields.CustomFields.AsEnumerable().First(t => t.Field<string>("MD_PROP_NAME") == column).Field<Guid>("MD_PROP_UID");
                        csRwo.PROJ_UID = new Guid(project.Field<string>("PROJ_UID"));
                        projectdS.ProjectCustomFields.Rows.Add(csRwo);
                    }
                    if (!(customFields.CustomFields.AsEnumerable().First(t => t.Field<string>("MD_PROP_NAME") == column) as Microsoft.Office.Project.Server.Schema.
                        CustomFieldDataSet.CustomFieldsRow)
                        .IsMD_LOOKUP_TABLE_UIDNull())
                    {
                        if (project[column] != null && project[column] != System.DBNull.Value)
                            csRwo.CODE_VALUE = column1.LookupTableItems.First(t => t.BoxedValue.ToString() == project.Field<string>(column).Replace("'","")).ValueMember;
                    }
                    else
                    {
                        var type = customFields.CustomFields.AsEnumerable().First(t => t.Field<string>("MD_PROP_NAME") == column).Field<byte>("MD_PROP_TYPE_ENUM");
                        var name = customFields.CustomFields.AsEnumerable().First(t => t.Field<string>("MD_PROP_NAME") == column).Field<string>("MD_PROP_NAME");
                        SetCustomFieldVal(project, csRwo, type, name);
                    }


                }

                updateResult.success = UpdateProject(projectdS, Guid.NewGuid());


            }
            catch (Exception ex)
            {
            }
            finally
            {




            }
            return updateResult;

        }

        public bool UpdateProject(Microsoft.Office.Project.Server.Schema.ProjectDataSet projectDs, Guid sessionId)
        {
            bool result = true;
            sessionId = Guid.NewGuid();
            Guid projUid = projectDs.Project[0].PROJ_UID;
            var PJWebPartContext = _pj.PSI;
            string projectName = projectDs.Project[0].PROJ_NAME;
            Console.WriteLine("update project team started for {0}", projectName);
            Microsoft.Office.Project.Server.Schema.ProjectDataSet deltaDataSet = new Microsoft.Office.Project.Server.Schema.ProjectDataSet();
            var jobuid = Guid.NewGuid();
            if (projectDs.GetChanges() != null)
            {

                if (projectDs.GetChanges(DataRowState.Added) != null)
                {
                    deltaDataSet.Merge(projectDs.GetChanges(DataRowState.Added));

                    try
                    {
                        Guid jobGuid = Guid.NewGuid();
                        PJWebPartContext.ProjectWebService.CheckOutProject(projUid, sessionId, "");
                        PJWebPartContext.ProjectWebService.QueueAddToProject(jobGuid, sessionId, deltaDataSet, false);
                        // Wait for the Project Server Queuing System to create the project.
                        if (QueueHelper.WaitForQueueJobCompletion(jobGuid,  (int)Microsoft.Office.Project.Server.Library.QueueConstants.QueueMsgType.ProjectUpdate,PJWebPartContext))
                        {
                            //jobGuid = Guid.NewGuid();

                            PJWebPartContext.ProjectWebService.QueuePublish(Guid.NewGuid(), projUid, false, null);
                            if (QueueHelper.WaitForQueueJobCompletion(jobGuid, (int)Microsoft.Office.Project.Server.Library.QueueConstants.QueueMsgType.ProjectPublish, PJWebPartContext))
                            {
                                // nulll is what we think a projet site publish without site creation
                                
                                PJWebPartContext.ProjectWebService.QueueCheckInProject(jobGuid, projUid, true, Guid.NewGuid(), "");
                                //if (WaitForQueueJobCompletion(jobGuid, Guid.NewGuid(), (int)Microsoft.Office.Project.Server.Library.QueueConstants.QueueMsgType.ProjectPublish))
                                if (true)
                                {
                                    Console.WriteLine("update project done successfully for {0}", projectName);
                                    
                                }
                                else
                                {
                                    Console.WriteLine(
                                               "update project done queue error for {0}", projectName);
                                    return false;
                                }
                            }
                            else
                            {
                                Console.WriteLine(
                                                "update project  done queue error for {0}", projectName);
                                return false;

                            }

                        }
                        else
                        {
                            Console.WriteLine(
                                               "update project done queue error for {0}", projectName);
                            PJWebPartContext.ProjectWebService.QueuePublish(Guid.NewGuid(), projUid, false, null);
                            PJWebPartContext.ProjectWebService.QueueCheckInProject(Guid.NewGuid(), projUid, true, sessionId, "");
                            return false;
                        }
                    }
                    catch (Exception)
                    {
                        PJWebPartContext.ProjectWebService.QueuePublish(Guid.NewGuid(), projUid, false, null);
                        PJWebPartContext.ProjectWebService.QueueCheckInProject(Guid.NewGuid(), projUid, true, sessionId, "");
                        return false;
                    }

                }
                else
                {
                    Console.WriteLine("update project done successfully for {0}", projectName);

                }

                if (projectDs.GetChanges(DataRowState.Modified) != null)
                {
                    deltaDataSet = new Microsoft.Office.Project.Server.Schema.ProjectDataSet();
                    deltaDataSet.Merge(projectDs.GetChanges(DataRowState.Modified));


                    try
                    {
                        Guid jobGuid = Guid.NewGuid();
                        sessionId = Guid.NewGuid();
                        PJWebPartContext.ProjectWebService.CheckOutProject(projUid, sessionId, "");


                        PJWebPartContext.ProjectWebService.QueueUpdateProject(jobGuid, sessionId, deltaDataSet, false);
                        // Wait for the Project Server Queuing System to create the project.
                        if (QueueHelper.WaitForQueueJobCompletion(jobGuid, (int)Microsoft.Office.Project.Server.Library.QueueConstants.QueueMsgType.ProjectUpdate, PJWebPartContext))
                        {
                            PJWebPartContext.ProjectWebService.QueuePublish(Guid.NewGuid(), projUid, false, null);

                            if (QueueHelper.WaitForQueueJobCompletion(jobGuid, (int)Microsoft.Office.Project.Server.Library.QueueConstants.QueueMsgType.ProjectPublish, PJWebPartContext))
                            {
                                
                                 PJWebPartContext.ProjectWebService.QueueCheckInProject(jobGuid, projUid, true, Guid.NewGuid(), "");
                                    
                                    //if (WaitForQueueJobCompletion(jobGuid, Guid.NewGuid(), (int)Microsoft.Office.Project.Server.Library.QueueConstants.QueueMsgType.ProjectPublish))
                                    if (true)
                                    {
                                        Console.WriteLine("update project done successfully for {0}", projectName);
                                    }
                                    else
                                    {
                                        Console.WriteLine(
                                                   "update project done queue error for {0}", projectName);
                                        PJWebPartContext.ProjectWebService.QueuePublish(Guid.NewGuid(), projUid, false, null);
                                        PJWebPartContext.ProjectWebService.QueueCheckInProject(Guid.NewGuid(), projUid, true, sessionId, "");
                                        return false;
                                    }
                                }
                                else
                                {
                                    PJWebPartContext.ProjectWebService.QueuePublish(Guid.NewGuid(), projUid, false, null);
                                }
                            }
                           
                        else
                        {
                            Console.WriteLine(
                                               "update project done queue error for {0}", projectName);
                            PJWebPartContext.ProjectWebService.QueuePublish(Guid.NewGuid(), projUid, false, null);
                            PJWebPartContext.ProjectWebService.QueueCheckInProject(Guid.NewGuid(), projUid, true, sessionId, "");
                            return false;
                        }
                    }
                    catch (Exception)
                    {
                        PJWebPartContext.ProjectWebService.QueuePublish(Guid.NewGuid(), projUid, false, null);
                        PJWebPartContext.ProjectWebService.QueueCheckInProject(Guid.NewGuid(), projUid, true, sessionId, "");
                        return false;
                    }

                }
                else
                {
                    Console.WriteLine("update project done successfully for {0}", projectName);
                }

            }
            return result;
        }
        private void SetCustomFieldVal(DataRow project, Microsoft.Office.Project.Server.Schema.ProjectDataSet.ProjectCustomFieldsRow csRwo, byte type, string name)
        {
            switch (type)
            {
                case 6:
                case 27:
                case 4:
                    if (project[name] != null && project[name] != System.DBNull.Value)
                        csRwo.DATE_VALUE = Convert.ToDateTime(project[name]);
                    else
                    {
                        csRwo.SetDATE_VALUENull();
                    }
                    break;
                case 21:
                case 9:
                    if (project[name] != null && project[name] != System.DBNull.Value)
                        csRwo.TEXT_VALUE = project[name].ToString();
                    break;


                case 17:
                    if (project[name] != null && project[name] != System.DBNull.Value)
                        csRwo.FLAG_VALUE = Convert.ToBoolean(project[name].ToString());
                    break;
                case 15:
                    if (project[name] != null && project[name] != System.DBNull.Value)
                        csRwo.NUM_VALUE = Convert.ToDecimal(project.Field<string>(name));
                    break;
            }
        }
        private Microsoft.Office.Project.Server.Schema.CustomFieldDataSet GetCustomFields(System.Collections.Generic.List<string> columns)
        {
            Microsoft.Office.Project.Server.Schema.CustomFieldDataSet cfDataSet = new Microsoft.Office.Project.Server.Schema.CustomFieldDataSet();
            Microsoft.Office.Project.Server.Schema.CustomFieldDataSet output = new Microsoft.Office.Project.Server.Schema.CustomFieldDataSet();
            try
            {
                string tableName = cfDataSet.CustomFields.TableName;
                string nameColumn = cfDataSet.CustomFields.MD_PROP_NAMEColumn.ColumnName;
                string uidsecndaryColumnName = cfDataSet.CustomFields.MD_PROP_UID_SECONDARYColumn.ColumnName;
                string uidColumnName = cfDataSet.CustomFields.MD_PROP_UIDColumn.ColumnName;
                string typeColumnName = cfDataSet.CustomFields.MD_PROP_TYPE_ENUMColumn.ColumnName;
                string lookuptableuidName = cfDataSet.CustomFields.MD_LOOKUP_TABLE_UIDColumn.ColumnName;
                string ismultilineName = cfDataSet.CustomFields.MD_PROP_IS_MULTILINE_TEXTColumn.ColumnName;
                PSLib.Filter.FieldOperationType equal =
                              PSLib.Filter.FieldOperationType.Equal;
                PSLib.Filter cfFilter = new PSLib.Filter();
                cfFilter.FilterTableName = tableName;
                cfFilter.Fields.Add(new PSLib.Filter.Field(tableName, nameColumn, PSLib.Filter.SortOrderTypeEnum.None));
                cfFilter.Fields.Add(new PSLib.Filter.Field(tableName, uidColumnName, PSLib.Filter.SortOrderTypeEnum.None));
                cfFilter.Fields.Add(new PSLib.Filter.Field(tableName, uidsecndaryColumnName, PSLib.Filter.SortOrderTypeEnum.None));
                cfFilter.Fields.Add(new PSLib.Filter.Field(tableName, typeColumnName, PSLib.Filter.SortOrderTypeEnum.None));
                cfFilter.Fields.Add(new PSLib.Filter.Field(tableName, lookuptableuidName, PSLib.Filter.SortOrderTypeEnum.None));
                cfFilter.Fields.Add(new PSLib.Filter.Field(tableName, ismultilineName, PSLib.Filter.SortOrderTypeEnum.None));
                List<PSLib.Filter.IOperator> operands = new List<PSLib.Filter.IOperator>();

                foreach (var configField in columns)
                {
                    operands.Add(new PSLib.Filter.FieldOperator(PSLib.Filter.FieldOperationType.Equal, nameColumn, configField));
                }

                cfFilter.Criteria = new PSLib.Filter.LogicalOperator(PSLib.Filter.LogicalOperationType.Or, operands.ToArray());
                output = _pj.PSI.CustomFieldsWebService.ReadCustomFields(cfFilter.GetXml(), false);

            }
            catch (Exception ex)
            {

            }
            finally
            {

            }

            return output;
        }

        public void ProcessResult(HttpContext context)
        {
            Output = Newtonsoft.Json.JsonConvert.SerializeObject(Result);
        }
    }

    [Serializable]
    public class UpdateResult
    {
        public string debugError;

        
        public bool success { get; set; }
        public string error { get; set; }

        public Project project { get; set; } = new Project();

        
    }

    /// <summary>
    /// A settings class to manage view
    /// </summary>
    [Serializable]
    public class Settings
    {
        public string Name { get; set; }
        public bool IsReadOnly { get; set; }
        /// <summary>
        /// represents MD_PROP_TYPE in PSI
        /// </summary>
        public string CustomFieldType
        {
            get;
            set;
        }

        /// <summary>
        /// Lookup Table Items list that the grid's dropdown column can bind to
        /// </summary>
        public LookupTableDisplayItem[] LookupTableItems { get; set; }

        /// <summary>
        /// Identifies Rich Text column type
        /// </summary>
        public bool IsMultiLine { get; set; }

        /// <summary>
        /// Identifies if MultiSelect for LookupTable
        /// </summary>
        public bool IsMultiSelect { get; set; }
    }

    /// <summary>
    /// List item for a selection in a custom field lookup table
    /// </summary>
    [Serializable]
    public class LookupTableDisplayItem
    {
        private string myDisplayMember;
        private Guid myValueMember;
        private string myDataType;
        private object myBoxedValue;
        public LookupTableDisplayItem()
        {
        }
        /// <summary>
        /// List item for a selection in a custom field lookup table.
        /// </summary>
        /// <param name="valueMember">Guid of lookup table item</param>
        /// <param name="displayMember">Display text for lookup table item</param>
        /// <param name="dataType">Project Server datatype of selection<seealso cref="PSLibrary.PSDataType"/></param>
        /// <param name="boxedValue">The value of the selection boxed in an object</param>
        /// 
        public LookupTableDisplayItem(Guid valueMember,
                                      string displayMember,
                                      string dataType,
                                      object boxedValue)
        {
            myDisplayMember = displayMember;
            myValueMember = valueMember;
            myDataType = dataType;
            myBoxedValue = boxedValue;
        }
        /// <summary>
        /// Display text for the lookup table item
        /// </summary>
        public string DisplayMember
        {
            get
            {
                return myDisplayMember.Replace("'", "");
            }

            set
            {
                myDisplayMember = value;
            }

        }

        /// <summary>
        /// Guid of the lookup table item
        /// </summary>
        public Guid ValueMember
        {
            get
            {
                return myValueMember;
            }
            set
            {
                myValueMember = value;
            }
        }

        /// <summary>
        /// Project Server datatype of selection<seealso cref="PSLibrary.PSDataType"/>
        /// </summary>
        public string DataType
        {
            get
            {
                return myDataType;
            }
            set
            {
                myDataType = value;
            }
        }

        /// <summary>
        /// The value of the selection boxed in an object
        /// </summary>
        public object BoxedValue
        {
            get
            {
                if (myBoxedValue is string)
                {
                    return myBoxedValue.ToString().Replace("'", "");
                }
                return myBoxedValue;
            }

            set
            {
                myBoxedValue = value;
            }
        }

        public override bool Equals(object obj)
        {
            LookupTableDisplayItem other = obj as LookupTableDisplayItem;
            if (other == null) return false;
            else return this.BoxedValue == other.BoxedValue && this.DataType == other.DataType && other.DisplayMember == this.DisplayMember && other.ValueMember == this.ValueMember;
        }
        public override int GetHashCode()
        {
            return ValueMember.GetHashCode();
        }
        public LookupTableDisplayItem GetCopy()
        {
            return this.MemberwiseClone() as LookupTableDisplayItem;
        }
    }
}
