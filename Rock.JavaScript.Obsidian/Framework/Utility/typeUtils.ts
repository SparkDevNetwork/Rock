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
 * Make specific properties in T optional, and able to be set to undefined,
 * while leaving the rest of the properties untouched.
 *
 * @example
 * type Shape = {
 *   length: number,
 *   width: number
 * };
 *
 * type StaticShape = PartialProps<Shape, "width">;
 * // {
 * //   length: number,
 * //   width?: number
 * // }
 */
export type PartialProps<T, K extends keyof T> = Omit<T, K> & {
    [P in keyof Pick<T, K>]+?: T[P]
};

/**
 * Make all properties in T optional, and able to be set to undefined or null.
 *
 * @example
 * type Shape = {
 *   length: number,
 *   width: number
 * };
 *
 * type StaticShape = PartialNullable<Shape>;
 * // {
 * //   length?: number | null,
 * //   width?: number | null
 * // }
 */
export type PartialNullable<T> = {
    [K in keyof T]+?: T[K] | null
};

/**
 * Make specific properties in T optional, and able to be set to undefined or null,
 * while leaving the rest of the properties untouched.
 *
 * @example
 * type Shape = {
 *   length: number,
 *   width: number
 * };
 *
 * type StaticShape = PartialNullableProps<Shape, "width">;
 * // {
 * //   length: number,
 * //   width?: number | null
 * // }
 */
export type PartialNullableProps<T, K extends keyof T> = Omit<T, K> & {
    [P in keyof Pick<T, K>]+?: T[P] | null
};

/**
 * Make all properties in T have a specific type.
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
 * Make specific properties in T have a specific type,
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
export type UnwrapBuilderType<T> = NonNullable<T>;

export function buildTypeFrom<T>(): TypeBuilder<T> {
    return new TypeBuilder<T>();
}

class TypeBuilder<T> {
    /**
     * Make all properties in T required.
     *
     * @returns A new chainable builder.
     *
     * @example
     * type Shape = {
     *   length?: number | null,
     *   width?: number | null
     * };
     *
     * const newBuilder = builder.withAllRequiredProps();
     * // A type built with the newBuilder would have the structure:
     * // {
     * //   length: number | null,
     * //   width: number | null
     * // }
     */
    withAllRequiredProps(): TypeBuilder<Required<T>> {
        return new TypeBuilder<Required<T>>();
    }

    /**
     * Make specific properties in T required,
     * while leaving the rest of the properties untouched.
     *
     * @returns A new chainable builder.
     *
     * @example
     * type Shape = {
     *   length?: number | null,
     *   width?: number | null
     * };
     *
     * const newBuilder = builder.withRequiredProps("length");
     * // A type built with the newBuilder would have the structure:
     * // {
     * //   length: number | null,
     * //   width?: number | null
     * // }
     */
    withRequiredProps<K extends keyof T>(prop: K, ... props: K[]): TypeBuilder<RequiredProps<T, K>> {
        return new TypeBuilder<RequiredProps<T, K>>();
    }

    /**
     * Make all properties in T required, and unable to be set to undefined or null.
     *
     * @returns A new chainable builder.
     *
     * @example
     * type Shape = {
     *   length?: number | null,
     *   width?: number | null
     * };
     *
     * const newBuilder = builder.withAllRequiredNonNullableProps();
     * // A type built with the newBuilder would have the structure:
     * // {
     * //   length: number,
     * //   width: number
     * // }
     */
    withAllRequiredNonNullableProps(): TypeBuilder<RequiredNonNullable<T>> {
        return new TypeBuilder<RequiredNonNullable<T>>();
    }

    /**
     * Make specific properties in T required, and unable to be set to undefined or null,
     * while leaving the rest of the properties untouched.
     *
     * @returns A new chainable builder.
     *
     * @example
     * type Shape = {
     *   length?: number | null,
     *   width?: number | null
     * };
     *
     * const newBuilder = builder.withRequiredNonNullableProps("width");
     * // A type built with the newBuilder would have the structure:
     * // {
     * //   length?: number | null,
     * //   width: number
     * // }
     */
    withRequiredNonNullableProps<K extends keyof T>(prop: K, ... props: K[]): TypeBuilder<RequiredNonNullableProps<T, K>> {
        return new TypeBuilder<RequiredNonNullableProps<T, K>>();
    }

    /**
     * Make all properties in T optional, and able to be set to undefined.
     *
     * @returns A new chainable builder.
     *
     * @example
     * type Shape = {
     *   length: number,
     *   width: number
     * };
     *
     * const newBuilder = builder.withAllOptionalProps();
     * // A type built with the newBuilder would have the structure:
     * // {
     * //   length?: number,
     * //   width?: number
     * // }
     */
    withAllOptionalProps(): TypeBuilder<Partial<T>> {
        return new TypeBuilder<Partial<T>>();
    }

    /**
     * Make specific properties in T optional, and able to be set to undefined,
     * while leaving the rest of the properties untouched.
     *
     * @returns A new chainable builder.
     *
     * @example
     * type Shape = {
     *   length: number,
     *   width: number
     * };
     *
     * const newBuilder = builder.withOptionalProps("length");
     * // A type built with the newBuilder would have the structure:
     * // {
     * //   length?: number,
     * //   width: number
     * // }
     */
    withOptionalProps<K extends keyof T>(prop: K, ... props: K[]): TypeBuilder<PartialProps<T, K>> {
        return new TypeBuilder<PartialProps<T, K>>();
    }

    /**
     * Make all properties in T optional, and able to be set to undefined or null.
     *
     * @returns A new chainable builder.
     *
     * @example
     * type Shape = {
     *   length: number,
     *   width: number
     * };
     *
     * const newBuilder = builder.withAllOptionalNullableProps();
     * // A type built with the newBuilder would have the structure:
     * // {
     * //   length?: number,
     * //   width: number
     * // }
     */
    withAllOptionalNullableProps(): TypeBuilder<PartialNullable<T>> {
        return new TypeBuilder<PartialNullable<T>>();
    }

    /**
     * Make specific properties in T optional, and able to be set to undefined or null,
     * while leaving the rest of the properties untouched.
     *
     * @returns A new chainable builder.
     *
     * @example
     * type Shape = {
     *   length: number,
     *   width: number
     * };
     *
     * const newBuilder = builder.withOptionalNullableProps("length");
     * // A type built with the newBuilder would have the structure:
     * // {
     * //   length?: number | null,
     * //   width: number
     * // }
     */
    withOptionalNullableProps<K extends keyof T>(prop: K, ... props: K[]): TypeBuilder<PartialNullableProps<T, K>> {
        return new TypeBuilder<PartialNullableProps<T, K>>();
    }

    /**
     * Make specific or all properties in T have a specific type.
     *
     * @returns A new chainable builder.
     *
     * @example
     * type Shape = {
     *   length?: number | null,
     *   width?: number | null
     * };
     *
     * // For specific properties...
     * const newBuilder = builder.withOverriddenType<string>().forProps("width");
     * // A type built with the newBuilder would have the structure:
     * // {
     * //   length?: number | null,
     * //   width?: string
     * // }
     *
     * // For all properties...
     * const newBuilder = builder.withOverriddenType<string>().forAllProps();
     * // A type built with the newBuilder would have the structure:
     * // {
     * //   length?: string,
     * //   width?: string
     * // }
     */
    withOverriddenType<U>(): OverridePropertyTypeBuilder<T, U> {
        return new OverridePropertyTypeBuilder<T, U>();
    }

    /**
     * Returns undefined, but should be used to extract the built type with:
     *
     * @example
     * type Shape = {
     *   length?: number | null,
     *   width?: number | null
     * };
     *
     * const temp = buildTypeFrom<Shape>().withRequiredProps("length", "width").build();
     * type StaticShape = UnwrapBuilderType<typeof temp>;
     * // {
     * //   length: number | null,
     * //   width: number | null
     * // }
     */
    build(): T | undefined {
        return;
    }
}

class OverridePropertyTypeBuilder<T, U> {
    /**
     * Make specific properties in T be of type U,
     * while leaving the rest of the properties untouched.
     *
     * @returns A new chainable builder.
     *
     * @example
     * type Shape = {
     *   length?: number | null,
     *   width?: number | null
     * };
     *
     * const newBuilder = builder.withOverriddenType<string>().forProps("width");
     * // A type built with the newBuilder would have the structure:
     * // {
     * //   length?: number | null,
     * //   width?: string
     * // }
     */
    forProps<K extends keyof T>(prop: K, ... props: K[]): TypeBuilder<OverrideTypeProps<T, K, U>> {
        return new TypeBuilder<OverrideTypeProps<T, K, U>>();
    }

    /**
     * Make all properties in T be of type U.
     *
     * @returns A new chainable builder.
     *
     * @example
     * type Shape = {
     *   length?: number | null,
     *   width?: number | null
     * };
     *
     * const newBuilder = builder.withOverriddenType<string>().forAllProps();
     * // A type built with the newBuilder would have the structure:
     * // {
     * //   length?: string,
     * //   width?: string
     * // }
     */
    forAllProps(): TypeBuilder<OverrideType<T, U>> {
        return new TypeBuilder<OverrideType<T, U>>();
    }
}