//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.Collections.Generic;
using System.Xml.Linq;

namespace Rock.PayFlowPro.Reporting
{
    /// <summary>
    /// 
    /// </summary>
    public class SearchRequest
    {
        public string SearchName { get; set; }
        public Dictionary<string, string> ReportParameters { get; set; }
        public int PageSize { get; set; }

        public SearchRequest()
        {
            ReportParameters = new Dictionary<string,string>();
            PageSize = 50;
        }

        public SearchRequest( string searchName )
            : this()
        {
            SearchName = searchName;
        }

        public SearchRequest( string reportName, Dictionary<string, string> reportParameters )
            : this( reportName )
        {
            ReportParameters = reportParameters;
        }

        public XElement ToXmlElement()
        {
            var xElement = new XElement( "runSearchRequest",
                new XElement( "searchName", SearchName ) );

            foreach ( var param in ReportParameters )
            {
                xElement.Add( new XElement( "reportParam",
                    new XElement( "paramName", param.Key ),
                    new XElement( "paramValue", param.Value ) ) );
            }

            xElement.Add( new XElement( "pageSize", "50" ) );

            return xElement;
        }
    }
}
