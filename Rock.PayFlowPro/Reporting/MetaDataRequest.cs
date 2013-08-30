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
    public class MetaDataRequest
    {
        public string ReportId { get; set; }

        public MetaDataRequest( string reportId )
        {
            ReportId = reportId;
        }

        public XElement ToXmlElement()
        {
            return new XElement( "getMetaDataRequest",
                new XElement( "reportId", ReportId ) );
        }
    }
}
