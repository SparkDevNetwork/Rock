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

namespace Rock.AI.Agent.Classes.Common
{
    /// <summary>
    /// The result of a summarize operation with all the grouped result data.
    /// </summary>
    internal class SummaryResult
    {
        /// <summary>
        /// The dimensions used to group the data in the summary results.
        /// The first item in the array represents the primary grouping,
        /// the second item represents the secondary grouping, and so on.
        /// </summary>
        public List<string> GroupingDimensions { get; set; }

        /// <summary>
        /// Child summary groups that further break down the summary results.
        /// </summary>
        public List<SummaryGroupResult> Groups { get; set; }

        /// <summary>
        /// The total number of items in the summary result.
        /// </summary>
        public int? Total { get; set; }
    }

}
