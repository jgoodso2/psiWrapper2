using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Office.Project.PWA;
using System.Collections.Specialized;

namespace PwaPSIWrapper
{
    public interface IPwaCommandFactory
    {
        string PwaCommandDescription { get;  }
        string PwaCommandName { get;   }
        IPwaCommand MakePwaCommand(PJContext pj, NameValueCollection pwaInput);
    }
}
