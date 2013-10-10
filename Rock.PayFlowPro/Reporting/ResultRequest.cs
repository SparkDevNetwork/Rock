//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.Xml.Linq;

namespace Rock.PayFlowPro.Reporting
{
    /// <summary>
    /// 
    /// </summary>
    public class ResultRequest
    {
        public string ReportId { get; set; }

        public ResultRequest( string reportId )
        {
            ReportId = reportId;
        }

        public XElement ToXmlElement()
        {
            return new XElement( "getResultsRequest",
                new XElement( "reportId", ReportId ) );
        }
    }
}
