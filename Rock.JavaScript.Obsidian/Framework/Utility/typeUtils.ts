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
 * Make all properties in T nullable.
 *
 * @example
 * type Shape = {
 *   length: number,
 *   width: number
 * };
 *
 * type StaticShape = Nullable<Shape>;
 * // {
 * //   length: number | null,
 * //   width: number | null
 * // }
 */
type Nullable<T> = {
    [K in keyof T]: T[K] | null
};

/**
 * Make specific properties in T required,
 * while leaving the rest of the properties untouched.
 *
 * @example
 * type Shape = {
 *   length?: number | null,
 *   width?: number | null
 * };
 *
 * type StaticShape = RequiredProps<Shape, "length">;
 * // {
 * //   length: number | null,
 * //   width?: number | null
 * // }
 */
export type RequiredProps<T, K extends keyof T> = Omit<T, K> & {
    [P in keyof Pick<T, K>]-?: T[P]
};

// /**
//  * Make all properties in T required, and unable to be set to undefined or null.
//  *
//  * @example
//  * type Shape = {
//  *   length?: number | null,
//  *   width?: number | null
//  * };
//  *
//  * type StaticShape = RequiredNonNullable<Shape>;
//  * // {
//  * //   length: number,
//  * //   width: number
//  * // }
//  */
// export type RequiredNonNullable<T> = {
//     [K in keyof T]-?: NonNullable<T[K]>
// };

/**
 * Make specific properties in T able to null,
 * while leaving the rest of the properties untouched.
 *
 * @example
 * type Shape = {
 *   length: number,
 *   width: number
 * };
 *
 * type StaticShape = NullableProps<Shape, "width">;
 * // {
 * //   length: number,
 * //   width: number | null
 * // }
 */
export type NullableProps<T, K extends keyof T> = Omit<T, K> & {
    [P in keyof Pick<T, K>]: T[P] | null
};

/**
 * Make properties in T able to be set to undefined
 *
 * @example
 * type Shape = {
 *   length: number | null,
 *   width: number | null
 * };
 *
 * type StaticShape = Undefinable<Shape>;
 * // {
 * //   length: number | null | undefined,
 * //   width: number | null | undefined
 * // }
 */
export type Undefinable<T> = {
    [K in keyof T]: T[K] | undefined
};

/**
 * Make properties in T able to be set to undefined
 *
 * @example
 * type Shape = {
 *   length: number | null,
 *   width: number | null
 * };
 *
 * type StaticShape = UndefinableProps<Shape, "length">;
 * // {
 * //   length: number | null | undefined,
 * //   width: number | null
 * // }
 */
export type UndefinableProps<T, K extends keyof T> = Omit<T, K> & {
    [P in keyof Pick<T, K>]: T[P] | undefined
};

/**
 * Returns type T if not of type U.
 *
 * Useful for removing union types (see NotNullable<T> and NotUndefinable<T>).
 */
type NotType<T, U> = T extends U ? never : T;

/**
 * Make properties in T not able to be set to undefined.
 *
 * @example
 * type Shape = {
 *   length: number | undefined,
 *   width: number | undefined
 * };
 *
 * type StaticShape = NotUndefinable<Shape>;
 * // {
 * //   length: number,
 * //   width: number
 * // }
 */
export type NotUndefinable<T> = {
    [K in keyof T]: NotType<T[K], undefined>
};

/**
 * Make specific properties in T not able to be set to undefined.
 *
 * @example
 * type Shape = {
 *   length: number | undefined,
 *   width: number | undefined
 * };
 *
 * type StaticShape = NotUndefinableProps<Shape, "length">;
 * // {
 * //   length: number,
 * //   width: number | undefined
 * // }
 */
export type NotUndefinableProps<T, K extends keyof T> = Omit<T, K> & {
    [P in keyof Pick<T, K>]: NotType<T[P], undefined>
};

/**
 * Make properties in T not able to be set to null.
 *
 * @example
 * type Shape = {
 *   length?: number | null,
 *   width?: number | null
 * };
 *
 * type StaticShape = NotNull<Shape>;
 * // {
 * //   length?: number,
 * //   width?: number
 * // }
 */
export type NotNullable<T> = {
    [K in keyof T]: NotType<T[K], null>
};

/**
 * Make specific properties in T not able to be set to null,
 * while leaving the rest of the properties untouched.
 *
 * @example
 * type Shape = {
 *   length?: number | null,
 *   width?: number | null
 * };
 *
 * type StaticShape = NotNullableProps<Shape, "length">;
 * // {
 * //   length: number,
 * //   width?: number | null
 * // }
 */
export type NotNullableProps<T, K extends keyof T> = Omit<T, K> & {
    [P in keyof Pick<T, K>]: NotType<T[P], null>
};

// /**
//  * Make specific properties in T required, and unable to be set to undefined or null,
//  * while leaving the rest of the properties untouched.
//  *
//  * @example
//  * type Shape = {
//  *   length?: number | null,
//  *   width?: number | null
//  * };
//  *
//  * type StaticShape = RequiredNonNullableProps<Shape, "length">;
//  * // {
//  * //   length: number,
//  * //   width?: number | null
//  * // }
//  */
// export type RequiredNonNullableProps<T, K extends keyof T> = Omit<T, K> & {
//     [P in keyof Pick<T, K>]-?: NonNullable<T[P]>
// };

/**
 * Make specific properties in T unable to be set to undefined or null,
 * while leaving the rest of the properties untouched.
 *
 * @example
 * type Shape = {
 *   length: number | null | undefined,
 *   width: number | null | undefined
 * };
 *
 * type StaticShape = NonNullableProps<Shape, "length">;
 * // {
 * //   length?: number,
 * //   width?: number | null
 * // }
 */
export type NonNullableProps<T, K extends keyof T> = Omit<T, K> & {
    [P in keyof Pick<T, K>]: NonNullable<T[P]>
};

/**
 * Make specific properties in T optional, and able to be set to undefined,
 * while leaving the rest of the properties untouched.
 *
 * @example
 * type Shape = {
 *   length: number,
 *   width: number
 * };
 *
 * type StaticShape = OptionalProps<Shape, "width">;
 * // {
 * //   length: number,
 * //   width?: number
 * // }
 */
export type OptionalProps<T, K extends keyof T> = Omit<T, K> & {
    [P in keyof Pick<T, K>]+?: T[P]
};

// /**
//  * Make properties in T optional, and able to be set to undefined or null.
//  *
//  * @example
//  * type Shape = {
//  *   length: number,
//  *   width: number
//  * };
//  *
//  * type StaticShape = PartialNullable<Shape>;
//  * // {
//  * //   length?: number | null,
//  * //   width?: number | null
//  * // }
//  */
// export type PartialNullable<T> = {
//     [K in keyof T]+?: T[K] | null
// };

// /**
//  * Make specific properties in T optional, and able to be set to undefined or null,
//  * while leaving the rest of the properties untouched.
//  *
//  * @example
//  * type Shape = {
//  *   length: number,
//  *   width: number
//  * };
//  *
//  * type StaticShape = PartialNullableProps<Shape, "width">;
//  * // {
//  * //   length: number,
//  * //   width?: number | null
//  * // }
//  */
// export type PartialNullableProps<T, K extends keyof T> = Omit<T, K> & {
//     [P in keyof Pick<T, K>]+?: T[P] | null
// };

/**
 * Make properties in T have a specific type U.
 *
 * @example
 * type Shape = {
 *   length: number,
 *   width: number
 * };
 *
 * type StaticShape = OverrideType<Shape, string>;
 * // {
 * //   length: number,
 * //   width: string
 * // }
 */
export type OverrideType<T, U> = {
    [P in keyof Pick<T, keyof T>]: U
};

/**
 * Make specific properties in T have a specific type U,
 * while leaving the rest of the properties untouched.
 *
 * @example
 * type Shape = {
 *   length: number,
 *   width: number
 * };
 *
 * type StaticShape = OverrideTypeProps<Shape, "width", string>;
 * // {
 * //   length: number,
 * //   width: string
 * // }
 */
export type OverrideTypeProps<T, K extends keyof T, U> = Omit<T, K> & {
    [P in keyof Pick<T, K>]: U
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
 * type ShapeProperties = KeysOfType<Shape, number>; // "length" | "width"; notice "height" does not appear because it can be a number, null, or undefined.
 *
 * // To include "height", you can either expand the second generic type argument to include null and undefined...
 * type ShapeProperties = KeysOfType<Shape, number | null | undefined>; // "length" | "width" | "height"
 *
 * // ...or you can convert Shape to a RequiredNonNullable type for the first generic type argument...
 * type ShapeProperties = KeysOfType<RequiredNonNullable<Shape>, number>; // "length" | "width" | "height"
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
 * type ShapeProperties = PropertiesOfType<Shape, number>;
 * // {
 * //   length: number;
 * //   width: number;
 * //   (Notice "height" does not appear because it can be a number, null, or undefined.)
 * // }
 *
 * // To include "height", you can either expand the second generic type argument to include null and undefined...
 * type ShapeProperties = PropertiesOfType<Shape, number | null | undefined>;
 * // {
 * //   length: number;
 * //   width: number;
 * //   height?: number | null | undefined;
 * // }
 *
 * // ...or you can convert Shape to a RequiredNonNullable type for the first generic type argument...
 * type ShapeProperties = PropertiesOfType<RequiredNonNullable<Shape>, number>;
 * // {
 * //   length: number;
 * //   width: number;
 * //   height: number; (This approach makes "height" a required, non-nullable property!)
 * // }
 */
export type PropertiesOfType<T, P> = Pick<T, KeysOfType<T, P>>;

/**
 * Use alongside buildTypeFrom<T>() to get a "dynamically" built type.
 *
 * @example
 * type Shape = {
 *   length?: number | null,
 *   width?: number | null
 * };
 *
 * const temp = buildTypeFrom<Shape>().withRequiredProps<"length" | "width">().build();
 * type StaticShape = UnwrapBuilderType<typeof temp>;
 * // {
 * //   length: number | null,
 * //   width: number | null
 * // }
 *
 * @example
 */
export type UnwrapTypeBuilder<T> = NonNullable<T>;

export class TypeBuilder {
    static fromType<T>(): FromTypeTypeBuilder<T> {
        return new FromTypeTypeBuilder<T>();
    }
}

// interface IPropertyTypeBuilder<T> {
//     nullable();
//     nonNullable();
//     required();

//     /**
//      * Make properties in T required, and unable to be set to undefined or null.
//      *
//      * @returns A new chainable builder.
//      *
//      * @example
//      * type Shape = {
//      *   length?: number | null,
//      *   width?: number | null
//      * };
//      *
//      * const newBuilder = builder.withAllRequiredNonNullableProps();
//      * // A type built with the newBuilder would have the structure:
//      * // {
//      * //   length: number,
//      * //   width: number
//      * // }
//      */
//     requiredAndNonNullable();
//     requiredAndNullable();

//     /**
//      * Make properties in T optional, and able to be set to undefined.
//      *
//      * @returns A new chainable builder.
//      *
//      * @example
//      * type Shape = {
//      *   length: number,
//      *   width: number
//      * };
//      *
//      * const newBuilder = builder.withAllOptionalProps();
//      * // A type built with the newBuilder would have the structure:
//      * // {
//      * //   length?: number,
//      * //   width?: number
//      * // }
//      */
//     optional();
//     optionalAndNonNullable();

//     /**
//      * Make specific properties in T optional, and able to be set to undefined or null,
//      * while leaving the rest of the properties untouched.
//      *
//      * @returns A new chainable builder.
//      *
//      * @example
//      * type Shape = {
//      *   length: number,
//      *   width: number
//      * };
//      *
//      * const newBuilder = builder.withOptionalNullableProps("length");
//      * // A type built with the newBuilder would have the structure:
//      * // {
//      * //   length?: number | null,
//      * //   width: number
//      * // }
//      */
//     optionalAndNullable();
//     type<U>();
// }

// class AllPropertiesTypeBuilder<T> implements IPropertyTypeBuilder<T> {
//     requiredAndNullable() {
//         throw new Error("Method not implemented.");
//     }
//     optionalAndNullable() {
//         throw new Error("Method not implemented.");
//     }
//     nonNullable() {
//         throw new Error("Method not implemented.");
//     }

//     optionalAndNonNullable() {
//         throw new Error("Method not implemented.");
//     }

//     nullable() {
//         return new FromTypeTypeBuilder<Nullable<T>>();
//     }

//     required() {
//         return new FromTypeTypeBuilder<Required<T>>();
//     }

//     requiredAndNonNullable(): FromTypeTypeBuilder<RequiredNonNullable<T>> {
//         return new FromTypeTypeBuilder<RequiredNonNullable<T>>();
//     }

//     optional() {
//         return new FromTypeTypeBuilder<Partial<T>>();
//     }

//     type<U>() {
//         return new FromTypeTypeBuilder<OverridePropertyTypeBuilder<T, U>>();
//     }
// }

// class PropertiesOfTypeTypeBuilder<T, P> implements IPropertyTypeBuilder<T> {
//     nonNullable() {
//         throw new Error("Method not implemented.");
//     }

//     requiredAndNonNullable() {
//         throw new Error("Method not implemented.");
//     }

//     optionalAndNonNullable() {
//         throw new Error("Method not implemented.");
//     }

//     nullable() {
//         throw new Error("Method not implemented.");
//     }

//     required() {
//         throw new Error("Method not implemented.");
//     }

//     optional() {
//         throw new Error("Method not implemented.");
//     }

//     type<U>() {
//         throw new Error("Method not implemented.");
//     }
// }

// class PropertiesWithNameTypeBuilder<T, K extends keyof T> implements IPropertyTypeBuilder<T> {
//     nonNullable() {
//         throw new Error("Method not implemented.");
//     }

//     optionalAndNonNullable() {
//         throw new Error("Method not implemented.");
//     }

//     nullable() {
//         throw new Error("Method not implemented.");
//     }

//     required() {
//         throw new Error("Method not implemented.");
//     }

//     requiredAndNonNullable(): FromTypeTypeBuilder<RequiredNonNullableProps<T, K>> {
//         return new FromTypeTypeBuilder<RequiredNonNullableProps<T, K>>();
//     }

//     optional() {
//         return new FromTypeTypeBuilder<PartialProps<T, K>>();
//     }

//     type<U>() {
//         throw new Error("Method not implemented.");
//     }
// }

// class FromTypeTypeBuilder<T> {
//     makeAllProperties(): AllPropertiesTypeBuilder<T> {
//         return new AllPropertiesTypeBuilder<T>();
//     }

//     makePropertiesOfType<P>(): PropertiesOfTypeTypeBuilder<T, P> {
//         return new PropertiesOfTypeTypeBuilder<T, P>();
//     }

//     makePropertiesWithName<K extends keyof T>(...props: K[]): PropertiesWithNameTypeBuilder<T, K> {
//         return new PropertiesWithNameTypeBuilder<T, K>();
//     }

//     /**
//      * Make specific properties in T optional, and able to be set to undefined or null,
//      * while leaving the rest of the properties untouched.
//      *
//      * @returns A new chainable builder.
//      *
//      * @example
//      * type Shape = {
//      *   length: number,
//      *   width: number
//      * };
//      *
//      * const newBuilder = builder.withOptionalNullableProps("length");
//      * // A type built with the newBuilder would have the structure:
//      * // {
//      * //   length?: number | null,
//      * //   width: number
//      * // }
//      */
//     setPropertyTypeToOptionalAndNullable(): OptionalAndNullablePropertyTypeBuilder<T> {
//         return new OptionalAndNullablePropertyTypeBuilder<T>();
//     }

//     /**
//      * Make specific or all properties in T have a specific type.
//      *
//      * @returns A new chainable builder.
//      *
//      * @example
//      * type Shape = {
//      *   length?: number | null,
//      *   width?: number | null
//      * };
//      *
//      * // For specific properties...
//      * const newBuilder = builder.withOverriddenType<string>().forProps("width");
//      * // A type built with the newBuilder would have the structure:
//      * // {
//      * //   length?: number | null,
//      * //   width?: string
//      * // }
//      *
//      * // For all properties...
//      * const newBuilder = builder.withOverriddenType<string>().forAllProps();
//      * // A type built with the newBuilder would have the structure:
//      * // {
//      * //   length?: string,
//      * //   width?: string
//      * // }
//      */
//     setPropertyTypeTo<U>(): OverridePropertyTypeBuilder<T, U> {
//         return new OverridePropertyTypeBuilder<T, U>();
//     }

//     /**
//      * Returns undefined, but should be used to extract the built type with:
//      *
//      * @example
//      * type Shape = {
//      *   length?: number | null,
//      *   width?: number | null
//      * };
//      *
//      * const temp = buildTypeFrom<Shape>().withRequiredProps("length", "width").build();
//      * type StaticShape = UnwrapBuilderType<typeof temp>;
//      * // {
//      * //   length: number | null,
//      * //   width: number | null
//      * // }
//      */
//     build(): T | undefined {
//         return;
//     }
// }

// class RequiredPropertyTypeBuilder<T> {

// }

// class RequiredAndNullablePropertyTypeBuilder<T> {

// }

// class OptionalPropertyTypeBuilder<T> {

// }

// class OptionalAndNullablePropertyTypeBuilder<T> {
//     /**
//      * Make all properties in T optional, and able to be set to undefined or null.
//      *
//      * @returns A new chainable builder.
//      *
//      * @example
//      * type Shape = {
//      *   length: number,
//      *   width: number
//      * };
//      *
//      * const newBuilder = builder.withAllOptionalNullableProps();
//      * // A type built with the newBuilder would have the structure:
//      * // {
//      * //   length?: number,
//      * //   width: number
//      * // }
//      */
//     forAllProperties(): FromTypeTypeBuilder<PartialNullable<T>> {
//         return new FromTypeTypeBuilder<PartialNullable<T>>();
//     }

//     /**
//      * Make specific properties in T optional, and able to be set to undefined or null,
//      * while leaving the rest of the properties untouched.
//      *
//      * @returns A new chainable builder.
//      *
//      * @example
//      * type Shape = {
//      *   length: number,
//      *   width: number
//      * };
//      *
//      * const newBuilder = builder.withOptionalNullableProps("length");
//      * // A type built with the newBuilder would have the structure:
//      * // {
//      * //   length?: number | null,
//      * //   width: number
//      * // }
//      */
//     forProperties<K extends keyof T>(prop: K, ... props: K[]): FromTypeTypeBuilder<PartialNullableProps<T, K>> {
//         return new FromTypeTypeBuilder<PartialNullableProps<T, K>>();
//     }

// }

// class NullablePropertyTypeBuilder<T> {

// }

// class OverridePropertyTypeBuilder<T, U> {
//     /**
//      * Make specific properties in T be of type U,
//      * while leaving the rest of the properties untouched.
//      *
//      * @returns A new chainable builder.
//      *
//      * @example
//      * type Shape = {
//      *   length?: number | null,
//      *   width?: number | null
//      * };
//      *
//      * const newBuilder = builder.withOverriddenType<string>().forProps("width");
//      * // A type built with the newBuilder would have the structure:
//      * // {
//      * //   length?: number | null,
//      * //   width?: string
//      * // }
//      */
//     forProperties<K extends keyof T>(... props: K[]): FromTypeTypeBuilder<OverrideTypeProps<T, K, U>> {
//         return new FromTypeTypeBuilder<OverrideTypeProps<T, K, U>>();
//     }

//     /**
//      * Make all properties in T be of type U.
//      *
//      * @returns A new chainable builder.
//      *
//      * @example
//      * type Shape = {
//      *   length?: number | null,
//      *   width?: number | null
//      * };
//      *
//      * const newBuilder = builder.withOverriddenType<string>().forAllProps();
//      * // A type built with the newBuilder would have the structure:
//      * // {
//      * //   length?: string,
//      * //   width?: string
//      * // }
//      */
//     forAllProperties(): FromTypeTypeBuilder<OverrideType<T, U>> {
//         return new FromTypeTypeBuilder<OverrideType<T, U>>();
//     }

//     /**
//      * Make all properties in T of type V be of type U.
//      *
//      * @returns A new chainable builder.
//      *
//      * @example
//      * type Shape = {
//      *   length: number,
//      *   width: number
//      * };
//      *
//      * const newBuilder = builder.withOverriddenType<string>().forPropsOfType<number>();
//      * // A type built with the newBuilder would have the structure:
//      * // {
//      * //   length: string,
//      * //   width: string
//      * // }
//      */
//     forPropertiesOfType<V>(): FromTypeTypeBuilder<OverrideTypeProps<T, keyof PropertiesOfType<T, V>, U>> {
//         return new FromTypeTypeBuilder<OverrideTypeProps<T, keyof PropertiesOfType<T, V>, U>>();
//     }

//     /**
//      * Make all properties in T of type V be of type U.
//      *
//      * @returns A new chainable builder.
//      *
//      * @example
//      * type Shape = {
//      *   length: number,
//      *   width: number
//      * };
//      *
//      * const newBuilder = builder.withOverriddenType<string>().forPropsOfType<number>();
//      * // A type built with the newBuilder would have the structure:
//      * // {
//      * //   length: string,
//      * //   width: string
//      * // }
//      */
//     forAllPropertiesOfTypeSmart<V>() {
//         type ReplaceType = OverrideTypeProps<T, keyof PropertiesOfType<T, V>, U>;
//         type ReplaceTypeOrNull = OverrideTypeProps<ReplaceType, keyof PropertiesOfType<ReplaceType, V | null>, U | null>;
//         type ReplaceTypeOrUndefined = OverrideTypeProps<ReplaceTypeOrNull, keyof PropertiesOfType<ReplaceTypeOrNull, V | undefined>, U | undefined>;
//         type ReplaceTypeOrNullOrUndefined = OverrideTypeProps<ReplaceTypeOrUndefined, keyof PropertiesOfType<ReplaceTypeOrUndefined, V | null | undefined>, U | null | undefined>;

//         // Do the same thing but with arrays of type U and V.
//         type ReplaceArrayType = OverrideTypeProps<ReplaceTypeOrNullOrUndefined, keyof PropertiesOfType<ReplaceTypeOrNullOrUndefined, V[]>, U[]>;
//         type ReplaceArrayTypeOrNull = OverrideTypeProps<ReplaceArrayType, keyof PropertiesOfType<ReplaceArrayType, V[] | null>, U[] | null>;
//         type ReplaceArrayTypeOrUndefined = OverrideTypeProps<InnerT, keyof PropertiesOfType<InnerT, V[] | undefined>, U[] | undefined>;
//         // Replaces properties in T of type V[] | null | undefined with type U[] | null | undefined.
//         type TypeArrayOrNullOrUndefinedToTypeArrayOrNullOrUndefined<InnerT> = OverrideTypeProps<InnerT, keyof PropertiesOfType<InnerT, V[] | null | undefined>, U[] | null | undefined>;
//         // Wrap them up into a smart type.
//         type SmartTypeArrayOverride<TInner> = TypeArrayOrNullOrUndefinedToTypeArrayOrNullOrUndefined<ReplaceArrayTypeOrUndefined<ReplaceArrayTypeOrNull<ReplaceArrayType<TInner>>>>;

//         // eslint-disable-next-line @typescript-eslint/ban-ts-comment
//         // @ts-ignore
//         return new FromTypeTypeBuilder<SmartTypeArrayOverride<SmartTypeOverride<T>>>();
//     }
// }

interface IPropertyTypeTypeBuilder<T> {
    nullable(): unknown;
    notNullable(): unknown;
    undefined(): unknown;
    notUndefined(): unknown;
    optional(): unknown;
    required(): unknown;
    type<PNew>(): unknown;
    defined(): unknown;
}

class AllPropertyTypeTypeBuilder<T> implements IPropertyTypeTypeBuilder<T> {
    private transform<TNew>(): MoreFromTypeTypeBuilder<TNew, AllPropertyTypeTypeBuilder<TNew>> {
        return new MoreFromTypeTypeBuilder<TNew, AllPropertyTypeTypeBuilder<TNew>>(new AllPropertyTypeTypeBuilder<TNew>());
    }

    nullable(): MoreFromTypeTypeBuilder<Nullable<T>, AllPropertyTypeTypeBuilder<Nullable<T>>> {
        return this.transform<Nullable<T>>();
    }

    notNullable(): MoreFromTypeTypeBuilder<NotNullable<T>, AllPropertyTypeTypeBuilder<NotNullable<T>>> {
        return this.transform<NotNullable<T>>();
    }

    undefined(): MoreFromTypeTypeBuilder<Undefinable<T>, AllPropertyTypeTypeBuilder<Undefinable<T>>> {
        return this.transform<Undefinable<T>>();
    }

    notUndefined(): MoreFromTypeTypeBuilder<NotUndefinable<T>, AllPropertyTypeTypeBuilder<NotUndefinable<T>>> {
        return this.transform<NotUndefinable<T>>();
    }

    optional(): MoreFromTypeTypeBuilder<Partial<T>, AllPropertyTypeTypeBuilder<Partial<T>>> {
        return this.transform<Partial<T>>();
    }

    required(): MoreFromTypeTypeBuilder<Required<T>, AllPropertyTypeTypeBuilder<Required<T>>> {
        return this.transform<Required<T>>();
    }

    type<PNew>(): MoreFromTypeTypeBuilder<OverrideType<T, PNew>, AllPropertyTypeTypeBuilder<OverrideType<T, PNew>>> {
        return this.transform<OverrideType<T, PNew>>();
    }

    defined(): MoreFromTypeTypeBuilder<NonNullable<T>, AllPropertyTypeTypeBuilder<NonNullable<T>>> {
        return this.transform<NonNullable<T>>();
    }
}

class PropertiesWithNamePropertyTypeTypeBuilder<T, K extends keyof T> implements IPropertyTypeTypeBuilder<T> {
    private transform<TNew>(): MoreFromTypeTypeBuilder<TNew, PropertiesWithNamePropertyTypeTypeBuilder<TNew, keyof TNew>> {
        return new MoreFromTypeTypeBuilder<TNew, PropertiesWithNamePropertyTypeTypeBuilder<TNew, keyof TNew>>(new PropertiesWithNamePropertyTypeTypeBuilder<TNew, keyof TNew>());
    }

    nullable(): MoreFromTypeTypeBuilder<NullableProps<T, K>, PropertiesWithNamePropertyTypeTypeBuilder<NullableProps<T, K>, keyof NullableProps<T, K>>> {
        return this.transform<NullableProps<T, K>>();
    }

    notNullable(): MoreFromTypeTypeBuilder<NotNullableProps<T, K>, PropertiesWithNamePropertyTypeTypeBuilder<NotNullableProps<T, K>, keyof NotNullableProps<T, K>>> {
        return this.transform<NotNullableProps<T, K>>();
    }

    undefined(): MoreFromTypeTypeBuilder<UndefinableProps<T, K>, PropertiesWithNamePropertyTypeTypeBuilder<UndefinableProps<T, K>, keyof UndefinableProps<T, K>>> {
        return this.transform<UndefinableProps<T, K>>();
    }

    notUndefined(): MoreFromTypeTypeBuilder<NotUndefinableProps<T, K>, PropertiesWithNamePropertyTypeTypeBuilder<NotUndefinableProps<T, K>, keyof NotUndefinableProps<T, K>>> {
        return this.transform<NotUndefinableProps<T, K>>();
    }

    optional(): MoreFromTypeTypeBuilder<OptionalProps<T, K>, PropertiesWithNamePropertyTypeTypeBuilder<OptionalProps<T, K>, keyof OptionalProps<T, K>>> {
        return this.transform<OptionalProps<T, K>>();
    }

    required(): MoreFromTypeTypeBuilder<RequiredProps<T, K>, PropertiesWithNamePropertyTypeTypeBuilder<RequiredProps<T, K>, keyof RequiredProps<T, K>>> {
        return this.transform<RequiredProps<T, K>>();
    }

    type<PNew>(): MoreFromTypeTypeBuilder<OverrideTypeProps<T, K, PNew>, PropertiesWithNamePropertyTypeTypeBuilder<OverrideTypeProps<T, K, PNew>, keyof OverrideTypeProps<T, K, PNew>>> {
        return this.transform<OverrideTypeProps<T, K, PNew>>();
    }

    defined(): MoreFromTypeTypeBuilder<NonNullableProps<T, K>, PropertiesWithNamePropertyTypeTypeBuilder<NonNullableProps<T, K>, keyof NonNullableProps<T, K>>> {
        return this.transform<NonNullableProps<T, K>>();
    }
}

class PropertiesOfTypePropertyTypeTypeBuilder<T, P> implements IPropertyTypeTypeBuilder<T> {
    private transform<TNew>(): MoreFromTypeTypeBuilder<TNew, PropertiesOfTypePropertyTypeTypeBuilder<TNew, P>> {
        return new MoreFromTypeTypeBuilder<TNew, PropertiesOfTypePropertyTypeTypeBuilder<TNew, P>>(new PropertiesOfTypePropertyTypeTypeBuilder<TNew, P>());
    }

    nullable(): MoreFromTypeTypeBuilder<NullableProps<T, keyof PropertiesOfType<T, P>>, PropertiesOfTypePropertyTypeTypeBuilder<NullableProps<T, keyof PropertiesOfType<T, P>>, P>> {
        return this.transform<NullableProps<T, keyof PropertiesOfType<T, P>>>();
    }

    notNullable(): MoreFromTypeTypeBuilder<NotNullableProps<T, keyof PropertiesOfType<T, P>>, PropertiesOfTypePropertyTypeTypeBuilder<NotNullableProps<T, keyof PropertiesOfType<T, P>>, P>> {
        return this.transform<NotNullableProps<T, keyof PropertiesOfType<T, P>>>();
    }

    undefined(): MoreFromTypeTypeBuilder<UndefinableProps<T, keyof PropertiesOfType<T, P>>, PropertiesOfTypePropertyTypeTypeBuilder<UndefinableProps<T, keyof PropertiesOfType<T, P>>, P>> {
        return this.transform<UndefinableProps<T, keyof PropertiesOfType<T, P>>>();
    }

    notUndefined(): MoreFromTypeTypeBuilder<NotUndefinableProps<T, keyof PropertiesOfType<T, P>>, PropertiesOfTypePropertyTypeTypeBuilder<NotUndefinableProps<T, keyof PropertiesOfType<T, P>>, P>> {
        return this.transform<NotUndefinableProps<T, keyof PropertiesOfType<T, P>>>();
    }

    optional(): MoreFromTypeTypeBuilder<OptionalProps<T, keyof PropertiesOfType<T, P>>, PropertiesOfTypePropertyTypeTypeBuilder<OptionalProps<T, keyof PropertiesOfType<T, P>>, P>> {
        return this.transform<OptionalProps<T, keyof PropertiesOfType<T, P>>>();
    }

    required(): MoreFromTypeTypeBuilder<RequiredProps<T, keyof PropertiesOfType<T, P>>, PropertiesOfTypePropertyTypeTypeBuilder<RequiredProps<T, keyof PropertiesOfType<T, P>>, P>> {
        return this.transform<RequiredProps<T, keyof PropertiesOfType<T, P>>>();
    }

    type<PNew>(): MoreFromTypeTypeBuilder<OverrideTypeProps<T, keyof PropertiesOfType<T, P>, PNew>, PropertiesOfTypePropertyTypeTypeBuilder<OverrideTypeProps<T, keyof PropertiesOfType<T, P>, PNew>, P>> {
        return this.transform<OverrideTypeProps<T, keyof PropertiesOfType<T, P>, PNew>>();
    }

    defined(): MoreFromTypeTypeBuilder<NonNullableProps<T, keyof PropertiesOfType<T, P>>, PropertiesOfTypePropertyTypeTypeBuilder<NonNullableProps<T, keyof PropertiesOfType<T, P>>, P>> {
        return this.transform<NonNullableProps<T, keyof PropertiesOfType<T, P>>>();
    }
}

class FromTypeTypeBuilder<T> {
    makeAllProperties(): AllPropertyTypeTypeBuilder<T> {
        return new AllPropertyTypeTypeBuilder<T>();
    }

    makePropertiesWithName<K extends keyof T>(..._names: K[]): PropertiesWithNamePropertyTypeTypeBuilder<T, K> {
        return new PropertiesWithNamePropertyTypeTypeBuilder<T, K>();
    }

    makePropertiesOfType<P>(): PropertiesOfTypePropertyTypeTypeBuilder<T, P> {
        return new PropertiesOfTypePropertyTypeTypeBuilder<T, P>();
    }
}

class MoreFromTypeTypeBuilder<T, B extends IPropertyTypeTypeBuilder<T>> {
    constructor(public and: B) {}

    makeAllProperties(): AllPropertyTypeTypeBuilder<T> {
        return new AllPropertyTypeTypeBuilder<T>();
    }

    makePropertiesWithName<K extends keyof T>(..._names: K[]): PropertiesWithNamePropertyTypeTypeBuilder<T, K> {
        return new PropertiesWithNamePropertyTypeTypeBuilder<T, K>();
    }

    makePropertiesOfType<P>(): PropertiesOfTypePropertyTypeTypeBuilder<T, P> {
        return new PropertiesOfTypePropertyTypeTypeBuilder<T, P>();
    }

    build(): T | undefined {
        return;
    }
}