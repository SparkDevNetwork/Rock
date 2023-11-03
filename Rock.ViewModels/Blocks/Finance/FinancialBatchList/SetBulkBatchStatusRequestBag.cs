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
    /// The parameter data for the SetBulkBatchStatus block action.
    /// </summary>
    public class SetBulkBatchStatusRequestBag
    {
        /// <summary>
        /// The batch identifiers to be opened or closed.
        /// </summary>
        public List<string> Keys { get; set; }

        /// <summary>
        /// If <c>true</c> then the batches will be opened, otherwise <c>false</c>.
        /// </summary>
        public bool Open { get; set; }
    }
}
