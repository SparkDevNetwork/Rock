//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Rock.CyberSource.Reporting
{
    /// <summary>
    /// 
    /// </summary>
    public class DataResponse
    {
        public List<List<string>> Rows { get; set; }

        public DataResponse( XDocument xmlResponse )
        {
            var dataResponse = xmlResponse.Root.Elements()
                .Where( x => x.Name == "getDataResponse" ).FirstOrDefault();

            Rows = new List<List<string>>();

            if ( dataResponse != null )
            {
                foreach ( var rowElement in dataResponse.Elements( "reportDataRow" ) )
                {
                    var row = new List<string>();
                    Rows.Add( row );

                    foreach ( var colElement in rowElement.Elements( "columnData" ) )
                    {
                        row.Add( (string)colElement.Element( "data" ) );
                    }
                }
            }
        }
    }
}
