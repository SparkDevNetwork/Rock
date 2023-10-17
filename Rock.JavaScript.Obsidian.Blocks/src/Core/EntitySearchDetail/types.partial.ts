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

export const enum NavigationUrlKey {
    ParentPage = "ParentPage"
}

/**
 * Defines additions to the EntitySearchBag that are used internally by
 * the block but don't get sent down from the C# code.
 */
export type EntitySearchBagAdditions = {
    /** Determines if the where expression has been enabled in the editor. */
    isWhereEnabled?: boolean;

    /** Determines if the group by expression has been enabled in the editor. */
    isGroupByEnabled?: boolean;

    /** Determines if the select expression has been enabled in the editor. */
    isSelectEnabled?: boolean;

    /** Determines if the select many expression has been enabled in the editor. */
    isSelectManyEnabled?: boolean;

    /** Determines if the order by expression has been enabled in the editor. */
    isOrderByEnabled?: boolean;
};
