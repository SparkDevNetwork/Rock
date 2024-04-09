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

namespace Rock.ViewModels.Blocks.Finance.FinancialBatchList
{
    /// <summary>
    /// The response data for the SetBulkBatchStatus block action.
    /// </summary>
    public class SetBulkBatchStatusResponseBag
    {
        /// <summary>
        /// <c>true</c> if the operation the status of the batches was updated
        /// or <c>false</c> if an error occurred.
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// The message that should be displayed to the individual.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// An optional list of errors that could be displayed to provide more
        /// context about what happened.
        /// </summary>
        public List<string> Errors { get; set; }
    }
}
