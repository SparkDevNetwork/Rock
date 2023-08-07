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

import { FamilyPreRegistrationPersonBag } from "@Obsidian/ViewModels/Blocks/Crm/FamilyPreRegistration/familyPreRegistrationPersonBag";
import { FamilyPreRegistrationCreateAccountRequestBag } from "@Obsidian/ViewModels/Blocks/Crm/FamilyPreRegistration/familyPreRegistrationCreateAccountRequestBag";

/**
 * Utility type that makes specific properties in T required and non-nullable.
 *
 * @example
 * type Shape = {
 *  length?: number | null,
 *  width?: number | null
 * };
 *
 * type StaticShape = RequiredProperties<Shape, "length" | "width">;
 * // {
 * //   length: number,
 * //   width: number
 * // }
 */
export type RequiredProperties<T, K extends keyof T> = Omit<T, K> & {
    [L in keyof Pick<T, K>]-?: NonNullable<T[L]>
};

/**
 * Represents a person pre-registration request.
 */
export type PersonRequestBag = RequiredProperties<
    FamilyPreRegistrationPersonBag,
    "firstName"
    | "lastName"
    | "email"
    | "mobilePhone"
    | "mobilePhoneCountryCode"
    | "attributeValues"
>;

/**
 * Represents a child pre-registration request.
 */
export type ChildRequestBag = RequiredProperties<PersonRequestBag, "familyRoleGuid">;

/**
 * Represents a create account request.
 */
export type CreateAccountRequest = RequiredProperties<
    FamilyPreRegistrationCreateAccountRequestBag,
    "username"
    | "password"
>;

/**
 * Utility type that returns the union of TObj properties that are of type TKey.
 *
 * @example
 * type Shape = {
 *   name: string;
 *   length: number;
 *   width: number;
 * };
 *
 * type ShapeNumberProperties = KeysOfType<Shape, number>; // "length" | "width"
 */
export type KeysOfType<TObj, TKey> = {
    [K in keyof TObj]: TObj[K] extends TKey ? K : never;
}[keyof TObj];

/**
 * Utility type that returns a new type from TObj with properties of type TKey.
 *
 * @example
 * type Shape = {
 *   name: string;
 *   length: number;
 *   width: number;
 * };
 *
 * type ShapeWithNumberProperties = PropertiesOfType<Shape, number>;
 * // {
 * //   length: number;
 * //   width: number;
 * // }
 */
export type PropertiesOfType<TObj, TKey> = Pick<TObj, KeysOfType<TObj, TKey>>;
