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
using System.Xml.Linq;

namespace Rock.PayFlowPro.Reporting
{
    /// <summary>
    /// 
    /// </summary>
    public class ReportRequest
    {
        public string ReportName { get; set; }
        public Dictionary<string, string> ReportParameters { get; set; }
        public int PageSize { get; set; }

        public ReportRequest()
        {
            ReportParameters = new Dictionary<string,string>();
            PageSize = 50;
        }

        public ReportRequest( string reportName )
            : this()
        {
            ReportName = reportName;
        }

        public ReportRequest( string reportName, Dictionary<string, string> reportParameters )
            : this( reportName )
        {
            ReportParameters = reportParameters;
        }

        public XElement ToXmlElement()
        {
            var xElement = new XElement( "runReportRequest",
                new XElement( "reportName", ReportName ) );

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
