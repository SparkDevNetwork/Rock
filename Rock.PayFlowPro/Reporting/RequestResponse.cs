//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.Linq;
using System.Xml.Linq;

namespace Rock.PayFlowPro.Reporting
{
    /// <summary>
    /// 
    /// </summary>
    public class RequestResponse
    {
        public string Code { get; set; }
        public string Message { get; set; }

        public RequestResponse( XDocument xmlResponse )
        {
            var baseResponse = xmlResponse.Root.Elements().Where( x =>
                x.Name == "baseResponse" ).FirstOrDefault();
            if ( baseResponse != null )
            {
                Code = (string)baseResponse.Element( "responseCode" );
                Message = (string)baseResponse.Element( "responseMsg" );
            }
        }
    }
}
