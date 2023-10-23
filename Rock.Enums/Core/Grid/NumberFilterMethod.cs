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

namespace Rock.Enums.Core.Grid
{
    /// <summary>
    /// The filtering method used for a number column filter.
    /// </summary>
    public enum NumberFilterMethod
    {
        /// <summary>
        /// The cell value must exactly match the filter value.
        /// </summary>
        Equals = 0,

        /// <summary>
        /// The cell value must not match the filter value.
        /// </summary>
        DoesNotEqual = 1,

        /// <summary>
        /// The cell value must be greater than the filter value.
        /// </summary>
        GreaterThan = 2,

        /// <summary>
        /// The cell value must be greater than or equal to the filter value.
        /// </summary>
        GreaterThanOrEqual = 3,

        /// <summary>
        /// The cell value must be less than the filter value.
        /// </summary>
        LessThan = 4,

        /// <summary>
        /// The cell value must be less than or equal to the filter value.
        /// </summary>
        LessThanOrEqual = 5,

        /// <summary>
        /// The cell value must be greater than or equal to the lower filter
        /// value and less than or equal to the upper filter value.
        /// </summary>
        Between = 6,

        /// <summary>
        /// The cell value must be in the top N values.
        /// </summary>
        TopN = 7,

        /// <summary>
        /// The cell value must be above the calculated average value.
        /// </summary>
        AboveAverage = 8,

        /// <summary>
        /// The cell value must be below the calculate average value.
        /// </summary>
        BelowAverage = 9
    }
}
