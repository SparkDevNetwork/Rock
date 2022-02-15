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
using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// ControlTotalResult DTO class for REST controller.
    /// </summary>
    [RockClientInclude( "Control Total Result from ~api/FinancialBatches/GetControlTotals/{id}" )]
    public class ControlTotalResult
    {
        /// <summary>
        /// Gets or sets the financial batch identifier.
        /// </summary>
        /// <value>
        /// The financial batch identifier.
        /// </value>
        public int FinancialBatchId { get; set; }

        /// <summary>
        /// Gets or sets the control total count.
        /// </summary>
        /// <value>
        /// The control total count.
        /// </value>
        public int ControlTotalCount { get; set; }

        /// <summary>
        /// Gets or sets the control total amount.
        /// </summary>
        /// <value>
        /// The control total amount.
        /// </value>
        public decimal ControlTotalAmount { get; set; }
    }
}