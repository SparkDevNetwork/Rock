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
 * Type representing the cache strategy for this item.
 * **Copied from Rock.Utility.RockCacheabilityType**: Please keep in sync.
 */
export const enum RockCacheabilityType {
    /* Represents the public Cache-Control header */
    Public = 0,
    /* Represents the private Cache-Control header */
    Private = 1,
    /* Represents the no-cache Cache-Control header */
    NoCache = 2,
    /* Represents the no-store Cache-Control header */
    NoStore = 3
}
