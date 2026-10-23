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

/** The way a discount reduces the registration cost. */
export const enum DiscountType {
    Percentage = "Percentage",
    Amount = "Amount"
}

/** Describes a field that was dragged to a new position within its form. */
export type FieldReorder = {
    /** The unique identifier of the field that moved. */
    fieldGuid: string;

    /** The unique identifier of the field it was placed before, or null when moved to the end. */
    beforeFieldGuid: string | null;
};
