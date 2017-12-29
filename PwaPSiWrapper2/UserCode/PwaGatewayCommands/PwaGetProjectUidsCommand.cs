using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Project.PWA;
using PwaPSIWrapper.UserCode.PwaGatewayCommands.Entity;
using System.Data;
using System.Data.SqlClient;
using System.Web.Script.Serialization;
using System.Configuration;
using System.Web;
using PwaPSIWrapper.Configuration;

namespace PwaPSIWrapper.UserCode.PwaGatewayCommands
{
    public class PwaGetProjectUidsCommand : IPwaCommand, IPwaCommandFactory, IPwaOutput
    {
        public string Output
        {
            get;
            set;
        }

        public string PwaCommandDescription
        {
            get
            {
                return "PwaUpdateProjectsCustomFieldsCommand - returns Project UIthat have resource plans";
            }
        }

        public string PwaCommandName
        {
            get
            {
                return "PwaGetProjectUidsCommand";
            }
        }

        public PwaGetProjectsUidsInput PwaInput;
        private PJContext _pj;
        private DataTable OutputDataSet;

        public void Execute()
        {
            OutputDataSet = GetProjectsWithResourcePlansForResource(PwaInput.Ruid);
            
        }

        public IPwaCommand MakePwaCommand(PJContext pj, NameValueCollection pwaInput)
        {
            return new PwaGetProjectUidsCommand() { _pj = pj, PwaInput = (PwaGetProjectsUidsInput)new PwaGetProjectsUidsInput(pwaInput).ParseInput() };
        }

        public void ProcessResult(HttpContext context)
        {
            Output = new JavaScriptSerializer().Serialize(GetJsonFromDataTable(OutputDataSet));
        }

        internal DataTable GetProjectsWithResourcePlansForResource(string ruid)
        {
            string connectionString = ConfigurationUtility.GetConnectionString("DataMart");
            //usp_GetPuidsForResourceWithResourcePlans
            DataTable csTable = new DataTable();
            //string puidsParam = string.Join(";", pUIDS.ToArray());
            SqlConnection sqlClient = new SqlConnection(connectionString);
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

        private List<Dictionary<string, Object>> GetJsonFromDataTable(DataTable dt)
        {
            System.Web.Script.Serialization.JavaScriptSerializer serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();
            Dictionary<string, object> row;
            foreach (DataRow dr in dt.Rows)
            {
                row = new Dictionary<string, object>();
                foreach (DataColumn col in dt.Columns)
                {
                    if (col.DataType == typeof(Single))
                    {
                        if (dr[col] == null || dr[col] == System.DBNull.Value)
                        {
                            row.Add(col.ColumnName, Convert.ToSingle(0).ToString("N2"));
                        }
                        else
                        {
                            row.Add(col.ColumnName, Convert.ToSingle(dr[col]).ToString("N2"));
                        }
                    }
                    else
                    {
                        row.Add(col.ColumnName, dr[col]);
                    }
                }
                rows.Add(row);
            }
            return rows;
        }
    }
}
