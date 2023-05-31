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
    /// The filtering method used for a text column filter.
    /// </summary>
    public enum TextFilterMethod
    {
        /// <summary>
        /// The case-insensitive cell value must start with the filter value.
        /// </summary>
        StartsWith = 0,

        /// <summary>
        /// The case-insensitive cell value must contain the filter value.
        /// </summary>
        Contains = 1,

        /// <summary>
        /// The case-insensitive cell value must not contain the filter value.
        /// </summary>
        DoesNotContain = 2,

        /// <summary>
        /// The case-insensitive cell value must end with the filter value.
        /// </summary>
        EndsWith = 3,

        /// <summary>
        /// The case-insensitive cell value must equal the filter value.
        /// </summary>
        Equals = 4,

        /// <summary>
        /// The case-insensitive cell value must not equal the filter value.
        /// </summary>
        DoesNotEqual = 5
    }
}
