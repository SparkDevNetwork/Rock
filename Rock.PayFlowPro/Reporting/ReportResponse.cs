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
using System.Linq;
using System.Xml.Linq;

namespace Rock.PayFlowPro.Reporting
{
    /// <summary>
    /// 
    /// </summary>
    public class ReportResponse
    {
        public string ReportId { get; set; }
        public string StatusCode { get; set; }
        public string StatusMessage { get; set; }

        public ReportResponse( XDocument xmlResponse )
        {
            var reportResponse = xmlResponse.Root.Elements()
                .Where( x => x.Name == "runReportResponse" ).FirstOrDefault();

            if ( reportResponse != null )
            {
                ReportId = (string)reportResponse.Element( "reportId" );
                StatusCode = (string)reportResponse.Element( "statusCode" );
                StatusMessage = (string)reportResponse.Element( "statusMsg" );
            }
        }
    }
}
