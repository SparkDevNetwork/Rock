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

/**
 * The type of filter expressions that can be used when building filtering rules.
 */
export const enum FilterExpressionType {
    /** Simple expression filter. */
    Filter = 0,

    /**
     * A collection of expressions/conditions that all must match. Expressions
     * are "and'd" together.
     */
    GroupAll = 1,

    /**
     * A collection of expressions/conditions where at least one must match.
     * Expressions are "or'd" together.
     */
    GroupAny = 2,

    /**
     * A collection of expressions/conditions where all must be false. Expressions
     * are combined using a logical OR and the group result must be FALSE.
     */
    GroupAllFalse = 3,

    /**
     * A collection of expressions/conditions where at least one must be false.
     * Expressions are combined using a logical AND and the group result must
     * be FALSE.
     */
    GroupAnyFalse = 4
}
