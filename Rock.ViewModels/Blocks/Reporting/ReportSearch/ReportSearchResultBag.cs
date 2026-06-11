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

namespace Rock.ViewModels.Blocks.Reporting.ReportSearch
{
    /// <summary>
    /// Contains a report search result row.
    /// </summary>
    public class ReportSearchResultBag
    {
        /// <summary>
        /// Gets or sets the identifier of the report.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the report name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the display category structure that appears before the report link.
        /// </summary>
        public string Structure { get; set; }

        /// <summary>
        /// Gets or sets the target URL for the report.
        /// </summary>
        public string Url { get; set; }
    }
}
