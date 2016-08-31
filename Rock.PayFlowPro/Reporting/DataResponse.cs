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
