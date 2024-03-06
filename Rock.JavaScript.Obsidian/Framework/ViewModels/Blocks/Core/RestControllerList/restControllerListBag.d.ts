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

export type RestControllerListBag = {
    /** Gets or sets the identifier of this entity. */
    id: number;

    /** Gets or sets the name of the REST controller. */
    name: string;

    /** Gets or sets the class name of the REST controller. */
    className?: string | null;

    /** Gets or sets the number of actions in the REST controller. */
    actions?: number | null;

    /** Gets or sets the number of actions with public caching headers in the REST controller. */
    actionsWithPublicCachingHeaders?: number | null;
};