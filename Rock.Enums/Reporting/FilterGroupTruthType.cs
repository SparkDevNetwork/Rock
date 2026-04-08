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

namespace Rock.Enums.Reporting
{
    /// <summary>
    /// Specifies whether the group evaluates for true or false conditions.
    /// </summary>
    public enum FilterGroupTruthType
    {
        /// <summary>
        /// The child filter logic should evaluate to true.
        /// </summary>
        True = 0,

        /// <summary>
        /// The child filter logic should evaluate to false.
        /// </summary>
        False = 1
    }
}
