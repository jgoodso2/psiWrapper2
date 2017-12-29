using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PwaPSIWrapper.UserCode.PwaGatewayCommands.Entity
{
    public class PwaUpdateProjectsCustomFieldsInput : IPwaCommandInput
    {
        public PwaUpdateProjectsCustomFieldsInput(NameValueCollection input)
        {
            Input = input;
        }
        public Dictionary<string, object> Projects { get; set; }

        public List<Settings> Columns { get; set; }

        public NameValueCollection Input
        {
            get; set;
        }

        public IPwaCommandInput ParseInput()
        {
            this.Projects = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(Input["Projects"]);
            this.Columns = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Settings>>(Input["Columns"]);
            return this;
        }
    }
}
