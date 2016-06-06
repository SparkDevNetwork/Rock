// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
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
                RowCount = ( (string)metaDataResponse.Element( "numberOfRows" ) ).AsInteger();
                PageCount = ( (string)metaDataResponse.Element( "numberOfPages" ) ).AsInteger();
                PageSize = ( (string)metaDataResponse.Element( "pageSize" ) ).AsInteger();
                ColumnCount = ( (string)metaDataResponse.Element( "numberOfColumns" ) ).AsInteger();

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
