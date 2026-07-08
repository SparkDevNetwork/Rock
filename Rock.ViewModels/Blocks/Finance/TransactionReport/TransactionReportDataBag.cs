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

using Rock.ViewModels.Core.Grid;

namespace Rock.ViewModels.Blocks.Finance.TransactionReport
{
    /// <summary>
    /// The transaction data returned for the current filter selections, both on initial load and
    /// when the individual applies a new filter.
    /// </summary>
    public class TransactionReportDataBag
    {
        /// <summary>
        /// Gets or sets the grid data describing the transactions that match the current filter.
        /// </summary>
        public GridDataBag GridData { get; set; }
    }
}
