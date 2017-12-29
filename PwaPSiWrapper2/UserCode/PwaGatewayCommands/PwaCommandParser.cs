using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Office.Project.PWA;
using System.Collections.Specialized;

namespace PwaPSIWrapper
{
    public class PwaCommandParser
    {
        public PJContext PwaConnection { get; set; }
        string _pwaCommandInput; 

        IEnumerable<IPwaCommandFactory> _availablePwaCommands;

        public PwaCommandParser(PJContext _pwaConnection, IEnumerable<IPwaCommandFactory> cmds )
        {
            PwaConnection = _pwaConnection; 
            _availablePwaCommands = cmds; 
        }

        internal IPwaCommand ParseCommand(NameValueCollection pwaArgs)
        {
            var requestedCommandName = string.IsNullOrEmpty(pwaArgs["method"]) ? "PwaNotFound" : pwaArgs["method"];


            var command = FindRequestedCommand(requestedCommandName);
            if (null == command)
                
                return new PwaPSIWrapper.PwaNotFoundCommand() { PwaCommandName = requestedCommandName };

            //return command.MakePwaCommand(args);
            return command.MakePwaCommand(PwaConnection, pwaArgs);
        }

        IPwaCommandFactory FindRequestedCommand(string commandName)
        {
            return _availablePwaCommands
                .FirstOrDefault(cmd => cmd.PwaCommandName == commandName);
        }

    }


    #region scrap
}
    #endregion


