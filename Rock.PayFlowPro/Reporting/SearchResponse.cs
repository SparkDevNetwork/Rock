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
    public class SearchResponse
    {
        public string ReportId { get; set; }
        public string StatusCode { get; set; }
        public string StatusMessage { get; set; }

        public SearchResponse( XDocument xmlResponse )
        {
            var reportResponse = xmlResponse.Root.Elements()
                .Where( x => x.Name == "runSearchResponse" ).FirstOrDefault();

            if ( reportResponse != null )
            {
                ReportId = (string)reportResponse.Element( "reportId" );
                StatusCode = (string)reportResponse.Element( "statusCode" );
                StatusMessage = (string)reportResponse.Element( "statusMsg" );
            }
        }
    }
}
