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

import { PublicAttributeBag } from "@Obsidian/ViewModels/Utility/publicAttributeBag";

/** PrayerRequest View Model */
export type PrayerRequestBag = {
    /** Gets or sets a flag indicating  whether or not comments can be made against the request. */
    allowComments?: boolean | null;

    /** Gets or sets a description of the way that God has answered the prayer. */
    answer?: string | null;

    /** Gets or sets the PersonId of the Rock.Model.Person who approved this prayer request. */
    approvedByPersonAliasId?: number | null;

    /** Gets or sets the date this prayer request was approved. */
    approvedOnDateTime?: string | null;

    /** Gets or sets the attributes. */
    attributes?: Record<string, PublicAttributeBag> | null;

    /** Gets or sets the attribute values. */
    attributeValues?: Record<string, string> | null;

    /** Gets or sets the campus identifier. */
    campusId?: number | null;

    /** Gets or sets the CategoryId of the Rock.Model.Category that the PrayerRequest belongs to. */
    categoryId?: number | null;

    /** Gets or sets the created by person alias identifier. */
    createdByPersonAliasId?: number | null;

    /** Gets or sets the created date time. */
    createdDateTime?: string | null;

    /** Gets or sets the email address of the person requesting prayer. */
    email?: string | null;

    /** Gets or sets the date that this prayer request was entered. */
    enteredDateTime?: string | null;

    /** Gets or sets the date that the prayer request expires.  */
    expirationDate?: string | null;

    /** Gets or sets the First Name of the person that this prayer request is about. This property is required. */
    firstName?: string | null;

    /** Gets or sets the number of times this request has been flagged. */
    flagCount?: number | null;

    /** Gets or sets the FullName of the request. */
    fullName?: string | null;

    /**
     * TODO: GET CLARIFICATION AND DOCUMENT
     * Gets or sets the group id.
     */
    groupId?: number | null;

    /** Gets or sets the identifier key of this entity. */
    idKey?: string | null;

    /** Gets or sets a flag indicating if this prayer request is active. */
    isActive?: boolean | null;

    /** Gets or sets a flag indicating if the prayer request has been approved.  */
    isApproved?: boolean | null;

    /** Gets or sets the flag indicating whether or not the request is public. */
    isPublic?: boolean | null;

    /** Gets or sets a flag indicating if this is an urgent prayer request. */
    isUrgent?: boolean | null;

    /** Gets or sets the DefinedValueId of the Rock.Model.DefinedValue that represents the Language for this prayer request. */
    languageValueId?: number | null;

    /** Gets or sets the Last Name of the person that this prayer request is about. This property is required. */
    lastName?: string | null;

    /** Gets or sets the modified by person alias identifier. */
    modifiedByPersonAliasId?: number | null;

    /** Gets or sets the modified date time. */
    modifiedDateTime?: string | null;

    /** Gets or sets the number of times that this prayer request has been prayed for. */
    prayerCount?: number | null;

    /** Gets or sets the PersonId of the Rock.Model.Person who is submitting the PrayerRequest */
    requestedByPersonAliasId?: number | null;

    /** Gets or sets the text/content of the request. */
    text?: string | null;
};
