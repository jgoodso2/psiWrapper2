
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using Microsoft.Office.Project.PWA;

namespace PwaPSIWrapper
{
    public class PwaNotFoundCommand : IPwaCommand, IPwaCommandFactory
    {
        Guid projUID; 
        public string PwaCommandDescription
        {
            get { return "This CommandNotFound"; }
            
        }

        public string PwaCommandName
        {
            get        { return "PwaNotFound"; } 
            set { PwaCommandName = value;  } 
        }

        public void Execute()
        {
            var oldProjuid = new Guid();
            //simulate working 
            Console.WriteLine("Old ProjUID = {0}, New ProjUID = {1}", oldProjuid, projUID);
        }

        public IPwaCommand MakePwaCommand(PJContext pj, NameValueCollection arguments)
        {
            return new PwaNotFoundCommand { projUID = new Guid() };
        }


        #region scrap
        

        #endregion


    }
}
