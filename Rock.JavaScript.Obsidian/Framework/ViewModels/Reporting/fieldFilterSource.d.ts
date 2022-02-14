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

import { FieldFilterSourceType } from "../../Reporting/fieldFilterSourceType";
import { Guid } from "../../Util/guid";
import { PublicFilterableAttribute } from "../publicFilterableAttribute";

/**
 * Describes a single source item an individual can pick from when building a
 * custom filter. This contains the information required to determine the
 * name to display, how to identify the source value and any other information
 * required to build the filter UI.
 */
export type FieldFilterSource = {
    /** The unique identifier of this source item. */
    guid: Guid;

    /** The source type this item represents. */
    type: FieldFilterSourceType;

    /** The information about the attribute when the type is Attribute. */
    attribute?: PublicFilterableAttribute | null;
};
