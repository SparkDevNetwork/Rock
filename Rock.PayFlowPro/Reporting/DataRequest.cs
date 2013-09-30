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
    public class DataRequest
    {
        public string ReportId { get; set; }
        public int PageNumber { get; set; }

        public DataRequest( string reportId, int pageNumber )
        {
            ReportId = reportId;
            PageNumber = pageNumber;
        }

        public XElement ToXmlElement()
        {
            return new XElement( "getDataRequest",
                new XElement( "reportId", ReportId ),
                new XElement( "pageNum", PageNumber.ToString() ) );
        }
    }
}
