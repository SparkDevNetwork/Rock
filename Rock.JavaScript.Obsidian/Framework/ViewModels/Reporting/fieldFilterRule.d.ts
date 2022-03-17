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

import { ComparisonType } from "../../Reporting/comparisonType";
import { FieldFilterSourceType } from "../../Reporting/fieldFilterSourceType";
import { Guid } from "../../Util/guid";

/**
 * Identifies a single filter rule/expression that will be used when determining
 * if some object matches the filter.
 */
export type FieldFilterRule = {
    /** The unique identifier of this rule in the system. */
    guid: Guid;

    /** The type of comparison to perform. */
    comparisonType: ComparisonType;

    /** The right-side value of the comparison. */
    value: string;

    /** The source of the left-side value of the comparison. */
    sourceType: FieldFilterSourceType;

    /** The unique identifier of the attribute to be used for the right-side value. */
    attributeGuid?: Guid | null;
};
