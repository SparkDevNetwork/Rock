//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Rock.PayFlowPro.Reporting
{
    /// <summary>
    /// 
    /// </summary>
    public class MetaDataResponse
    {
        public int RowCount { get; set; }
        public int PageCount { get; set; }
        public int PageSize { get; set; }
        public int ColumnCount { get; set; }
        public List<string> Columns { get; set; }

        public MetaDataResponse( XDocument xmlResponse )
        {
            var metaDataResponse = xmlResponse.Root.Elements()
                .Where( x => x.Name == "getMetaDataResponse" ).FirstOrDefault();

            if ( metaDataResponse != null )
            {
                RowCount = ( (string)metaDataResponse.Element( "numberOfRows" ) ).AsInteger() ?? 0;
                PageCount = ( (string)metaDataResponse.Element( "numberOfPages" ) ).AsInteger() ?? 0;
                PageSize = ( (string)metaDataResponse.Element( "pageSize" ) ).AsInteger() ?? 0;
                ColumnCount = ( (string)metaDataResponse.Element( "numberOfColumns" ) ).AsInteger() ?? 0;

                Columns = new List<string>();

                foreach ( var colElement in metaDataResponse.Descendants()
                    .Where( x => x.Name == "columnMetaData" ) )
                {
                    Columns.Add( (string)colElement.Element( "dataName" ) );
                }
            }
        }
    }
}
