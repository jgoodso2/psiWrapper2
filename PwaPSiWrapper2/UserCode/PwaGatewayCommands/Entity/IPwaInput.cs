using PwaPSIWrapper.UserCode.Misc;
using System.Collections.Specialized;

namespace PwaPSIWrapper
{
    public interface IPwaCommandInput
    {
        NameValueCollection Input { get; set; }

        IPwaCommandInput ParseInput();
    }
}