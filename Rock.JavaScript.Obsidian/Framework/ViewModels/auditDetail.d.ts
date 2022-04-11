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

import { Guid } from "../Util/guid";

/**
 * Contains the information required to show the audit panel details for a model.
 */
export type AuditDetail = {
    /** The identifier of the model. */
    id?: number | null;

    /** The unique identifier of the model. */
    guid?: Guid | null;

    /** The identifier of the person that created the model. */
    createdByPersonId?: number;

    /** The name of the person that created the model. */
    createdByName?: string | null;

    /** The time the model was created relative to now. */
    createdRelativeTime?: string | null;

    /** The identifier of the person that modified the model. */
    modifiedByPersonId?: number;

    /** The name of the person that modified the model. */
    modifiedByName?: string | null;

    /** The time the model was modified relative to now. */
    modifiedRelativeTime?: string | null;
};
