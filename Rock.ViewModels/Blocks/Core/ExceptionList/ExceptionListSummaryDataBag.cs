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

namespace Rock.ViewModels.Blocks.Core.ExceptionList
{
    /// <summary>
    /// A bag that contains summary data for the Exception List block.
    /// </summary>
    public class ExceptionListSummaryDataBag
    {
        /// <summary>
        /// Gets or sets the data that describes the contents of the grid.
        /// </summary>
        public GridDataBag GridDataBag { get; set; }

        /// <summary>
        /// Gets or sets the data that describes the count of exceptions per day.
        /// </summary>
        public ExceptionCountsPerDayBag ExceptionCountsPerDay { get; set; }
    }
}
