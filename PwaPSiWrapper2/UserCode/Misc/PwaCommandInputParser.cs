using Newtonsoft.Json;
using PwaPSIWrapper.UserCode.Misc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace PwaPSIWrapper.UserCode.PwaGatewayCommands.Entity
{
    public class PwaInputParser<T> 
    {
        public T ParseInput(PwaCommandContentType inputType,string input)
        {
            if(inputType == PwaCommandContentType.JSON)
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(input);
            }
            else
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                return (T) serializer.Deserialize(new XmlTextReader(new StringReader(input)));
            }
        }
    }
}
