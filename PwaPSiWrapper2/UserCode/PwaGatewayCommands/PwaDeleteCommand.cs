
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using Microsoft.Office.Project.PWA;
using PwaPSIWrapper.UserCode.PwaGatewayCommands.Entity;

namespace PwaPSIWrapper
{
    public class PwaDeleteCommand : IPwaCommand, IPwaCommandFactory
    {
       
        PJContext _pj;
        PwaCommandProjectInput input;
        public string PwaCommandDescription
        {
            get { return "PwaDelete command - needs a ProjectUID"; }
        }

        public string PwaCommandName
        {
            get        { return "PwaDelete"; } 
            set { PwaCommandName = value;  } 
        }

       
        public void Execute()
        {
            _pj.PSI.ProjectWebService.QueueDeleteProjects(Guid.NewGuid(), true, input.ProjUID, true);

        }

        public IPwaCommand MakePwaCommand(PJContext pj, string args)
        {
            //dummy some for now.  
            Guid[] guidArray = new Guid[3];
            guidArray[0] = Guid.NewGuid();
            guidArray[1] = Guid.NewGuid();
            guidArray[2] = Guid.NewGuid();

            return new PwaDeleteCommand { _pj = pj, input = new PwaInputParser<PwaCommandProjectInput>().ParseInput(UserCode.Misc.PwaCommandContentType.JSON, args) }; 
            
        }

        public IPwaCommand MakePwaCommand(PJContext pj, NameValueCollection pwaInput)
        {
            return new PwaDeleteCommand { _pj = pj, input = (PwaCommandProjectInput) new PwaCommandProjectInput(pwaInput).ParseInput() };
        }


        #region scrap


        #endregion


    }
}
