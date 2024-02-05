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
 * Make all properties in T required, and unable to be set to undefined or null.
 *
 * @example
 * type Shape = {
 *   length?: number | null,
 *   width?: number | null
 * };
 *
 * type StaticShape = RequiredNonNullable<Shape>;
 * // {
 * //   length: number,
 * //   width: number
 * // }
 */
export type RequiredNonNullable<T> = {
    [K in keyof T]-?: NonNullable<T[K]>
};

/**
 * Make specific properties in T required, and unable to be set to undefined or null,
 * while leaving the rest of the properties untouched.
 *
 * @example
 * type Shape = {
 *   length?: number | null,
 *   width?: number | null
 * };
 *
 * type StaticShape = RequiredNonNullableProps<Shape, "length">;
 * // {
 * //   length: number,
 * //   width?: number | null
 * // }
 */
export type RequiredNonNullableProps<T, K extends keyof T> = Omit<T, K> & {
    [P in keyof Pick<T, K>]-?: NonNullable<T[P]>
};

/**
 * Get the union of keys from T where the properties are of type P.
 *
 * @example
 * type Shape = {
 *   name: string;
 *   length: number;
 *   width: number;
 *   height?: number | null;
 * };
 *
 * type ShapeNumberProperties = KeysOfType<Shape, number>; // "length" | "width"; notice "height" does not appear because it can be a number, null, or undefined.
 *
 * // To include "height", you can either expand the second generic type argument to include null and undefined...
 * type ShapeNumberProperties = KeysOfType<Shape, number | null | undefined>; // "length" | "width" | "height"
 *
 * // ...or you can convert Shape to a RequiredNonNullable type for the first generic type argument...
 * type ShapeNumberProperties = KeysOfType<RequiredNonNullable<Shape>, number>; // "length" | "width" | "height"
 */
export type KeysOfType<T, P> = Exclude<{
    [K in keyof T]: T[K] extends P ? K : never;
}[keyof T], undefined>;

/**
 * Utility type that returns a new type from TObj with properties of type TKey.
 *
 * @example
 * type Shape = {
 *   name: string;
 *   length: number;
 *   width: number;
 *   height?: number | null;
 * };
 *
 * type ShapeNumberProperties = PropertiesOfType<Shape, number>;
 * // {
 * //   length: number;
 * //   width: number;
 * //   (Notice "height" does not appear because it can be a number, null, or undefined.)
 * // }
 *
 * // To include "height", you can either expand the second generic type argument to include null and undefined...
 * type ShapeNumberProperties = PropertiesOfType<Shape, number | null | undefined>;
 * // {
 * //   length: number;
 * //   width: number;
 * //   height?: number | null | undefined;
 * // }
 *
 * // ...or you can convert Shape to a RequiredNonNullable type for the first generic type argument...
 * type ShapeNumberProperties = PropertiesOfType<RequiredNonNullable<Shape>, number>;
 * // {
 * //   length: number;
 * //   width: number;
 * //   height: number; (This approach makes "height" a required, non-nullable property!)
 * // }
 */
export type PropertiesOfType<T, P> = Pick<T, KeysOfType<T, P>>;