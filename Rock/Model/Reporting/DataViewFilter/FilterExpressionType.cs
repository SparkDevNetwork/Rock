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

namespace Rock.Model
{
    /// <summary>
    /// Type of Filter entry
    /// </summary>
    public enum FilterExpressionType
    {
        /// <summary>
        /// Expression filter
        /// </summary>
        Filter = 0,

        /// <summary>
        /// A collection of expressions/conditions that must match and should be "and'd" together.
        /// </summary>
        GroupAll = 1,

        /// <summary>
        /// A collection of expressions/conditions where at least one condition/expression must match.  Expressions are "or'd" together.
        /// </summary>
        GroupAny = 2,

        /// <summary>
        /// A collection of expressions/conditions where all conditions must be false.  Expressions are combined using a logical OR and the group result must be FALSE.
        /// </summary>
        GroupAllFalse = 3,

        /// <summary>
        /// A collection of expressions/conditions where at least one condition must be false.  Expressions are combined using a logical AND and the group result must be FALSE.
        /// </summary>
        GroupAnyFalse = 4
    }
}
