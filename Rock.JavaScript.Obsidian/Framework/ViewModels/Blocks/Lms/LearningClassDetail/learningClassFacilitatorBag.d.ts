//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the Rock.CodeGeneration project
//     Changes to this file will be lost when the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
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

import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { PublicAttributeBag } from "@Obsidian/ViewModels/Utility/publicAttributeBag";

/** The facilitator details for the Learning Class Detail block. */
export type LearningClassFacilitatorBag = {
    /** Gets or sets the attributes. */
    attributes?: Record<string, PublicAttributeBag> | null;

    /** Gets or sets the attribute values. */
    attributeValues?: Record<string, string> | null;

    /** Gets or sets the facilitator list item. */
    facilitator?: ListItemBag | null;

    /** Gets or sets the email for the facilitator. */
    facilitatorEmail?: string | null;

    /** Gets or sets the facilitator name. */
    facilitatorName?: string | null;

    /** Gets or sets the facilitator Person Id. */
    facilitatorPersonId: number;

    /** Gets or sets the facilitator role. */
    facilitatorRole?: string | null;

    /** Gets or sets the identifier key of this entity. */
    idKey?: string | null;
};
