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
using System;

namespace Rock.BulkExport
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.BulkExport.ExportOptions" />
    public class FinancialTransactionExportOptions : ExportOptions
    {
        /// <summary>
        /// Optional filter to limit to transactions with a transaction date/time greater than or equal to <see cref="StartDateTime"/>
        /// </summary>
        /// <value>
        /// The modified since.
        /// </value>
        public DateTime? StartDateTime { get; set; }

        /// <summary>
        /// Optional filter to limit to transactions with a transaction date/time less than <see cref="EndDateTime"/>
        /// </summary>
        /// <value>
        /// The end date time.
        /// </value>
        public DateTime? EndDateTime { get; set; }
    }
}
