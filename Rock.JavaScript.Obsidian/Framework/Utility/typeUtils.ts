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

// #region Utility Types

/**
 * Make all properties in Type able to be set to null.
 *
 * @example
 * type Shape = {
 *   length: number;
 *   width?: number | undefined;
 * };
 *
 * type NewShape = Nullable<Shape>;
 * // {
 * //   length: number | null;
 * //   width?: number | null | undefined;
 * // }
 */
type Nullable<Type> = {
    [PropertyKey in keyof Type]: Type[PropertyKey] | null
};

/**
 * Make specific properties in Type required,
 * while leaving the rest of the properties untouched. (optional properties will become required and cannot be set to undefined)
 *
 * @example
 * type Shape = {
 *   length: number | null | undefined;
 *   width?: number | null | undefined;
 * };
 *
 * type NewShape = RequiredProps<Shape, "length" | "width">;
 * // {
 * //   length: number | null | undefined;
 * //   width: number | null;
 * // }
 */
export type RequiredProps<Type, RequiredPropertyKey extends keyof Type> = Omit<Type, RequiredPropertyKey> & {
    [PropertyKey in keyof Pick<Type, RequiredPropertyKey>]-?: Type[PropertyKey]
};

/**
 * Make properties of type PropertyType in Type able to be set to null,
 * while leaving the rest of the properties untouched.
 *
 * @example
 * type Shape = {
 *   length: number;
 *   width?: number | undefined;
 * };
 *
 * type NewShape = NullableProps<Shape, "width">;
 * // {
 * //   length: number;
 * //   width?: number | null | undefined;
 * // }
 */
export type NullableProps<Type, NullablePropertyKey extends keyof Type> = Omit<Type, NullablePropertyKey> & {
    [PropertyKey in keyof Pick<Type, NullablePropertyKey>]: Type[PropertyKey] | null
};

/**
 * Make all properties in Type able to be set to undefined.
 *
 * @example
 * type Shape = {
 *   length: number | null;
 *   width: number | null;
 * };
 *
 * type NewShape = Undefinable<Shape>;
 * // {
 * //   length: number | null | undefined;
 * //   width: number | null | undefined;
 * // }
 */
export type Undefinable<Type> = {
    [PropertyKey in keyof Type]: Type[PropertyKey] | undefined
};

/**
 * Make specific properties in Type able to be set to undefined.
 *
 * @example
 * type Shape = {
 *   length: number | null;
 *   width: number | null;
 * };
 *
 * type NewShape = UndefinableProps<Shape, "length">;
 * // {
 * //   length: number | null | undefined;
 * //   width: number | null;
 * // }
 */
export type UndefinableProps<Type, UndefinablePropertyKey extends keyof Type> = Omit<Type, UndefinablePropertyKey> & {
    [PropertyKey in keyof Pick<Type, UndefinablePropertyKey>]: Type[PropertyKey] | undefined
};

/**
 * Returns type T if not of type U.
 *
 * Useful for removing union types (see NotNullable<T> and NotUndefinable<T>).
 */
type NotType<Type, NotOfType> = Type extends NotOfType ? never : Type;

/**
 * Make all properties in Type not able to be set to undefined. (does nothing to optional properties)
 *
 * @example
 * type Shape = {
 *   length: number | undefined;
 *   width?: number | undefined;
 * };
 *
 * type NewShape = NotUndefinable<Shape>;
 * // {
 * //   length: number;
 * //   width?: number | undefined;
 * // }
 */
export type NotUndefinable<Type> = {
    [PropertyKey in keyof Type]: NotType<Type[PropertyKey], undefined>
};

/**
 * Make specific properties in Type not able to be set to undefined. (does nothing to optional properties)
 *
 * @example
 * type Shape = {
 *   length: number | undefined;
 *   width?: number | undefined;
 * };
 *
 * type NewShape = NotUndefinableProps<Shape, "length">;
 * // {
 * //   length: number;
 * //   width?: number | undefined;
 * // }
 */
export type NotUndefinableProps<Type, NotUndefinablePropertyKey extends keyof Type> = Omit<Type, NotUndefinablePropertyKey> & {
    [PropertyKey in keyof Pick<Type, NotUndefinablePropertyKey>]: NotType<Type[PropertyKey], undefined>
};

/**
 * Make properties in Type not able to be set to null.
 *
 * @example
 * type Shape = {
 *   length?: number | null;
 *   width?: number | null;
 * };
 *
 * type NewShape = NotNullable<Shape>;
 * // {
 * //   length?: number;
 * //   width?: number;
 * // }
 */
export type NotNullable<Type> = {
    [PropertyKey in keyof Type]: NotType<Type[PropertyKey], null>
};

/**
 * Make specific properties in Type not able to be set to null,
 * while leaving the rest of the properties untouched.
 *
 * @example
 * type Shape = {
 *   length?: number | null | undefined;
 *   width?: number | null | undefined;
 * };
 *
 * type NewShape = NotNullableProps<Shape, "length">;
 * // {
 * //   length?: number;
 * //   width?: number | null | undefined;
 * // }
 */
export type NotNullableProps<Type, NotNullablePropertyKey extends keyof Type> = Omit<Type, NotNullablePropertyKey> & {
    [PropertyKey in keyof Pick<Type, NotNullablePropertyKey>]: NotType<Type[PropertyKey], null>
};

/**
 * Make specific properties in Type unable to be set to null or undefined,
 * while leaving the rest of the properties untouched. (optional properties can still be set to undefined)
 *
 * @example
 * type Shape = {
 *   length: number | null | undefined;
 *   width?: number | null | undefined;
 * };
 *
 * type NewShape = DefinedProps<Shape, "length" | "width">;
 * // {
 * //   length: number;
 * //   width?: number | undefined;
 * // }
 */
export type DefinedProps<Type, DefinedPropertyKey extends keyof Type> = Omit<Type, DefinedPropertyKey> & {
    [PropertyKey in keyof Pick<Type, DefinedPropertyKey>]: NonNullable<Type[PropertyKey]>
};

/**
 * Make specific properties in Type optional,
 * while leaving the rest of the properties untouched.
 *
 * @example
 * type Shape = {
 *   length: number;
 *   width: number;
 * };
 *
 * type NewShape = OptionalProps<Shape, "width">;
 * // {
 * //   length: number;
 * //   width?: number | undefined;
 * // }
 */
export type OptionalProps<Type, PropertyKey extends keyof Type> = Omit<Type, PropertyKey> & {
    [Property in keyof Pick<Type, PropertyKey>]+?: Type[Property]
};

/**
 * Make all properties in Type be of type PropertyType.
 *
 * @example
 * type Shape = {
 *   length: number;
 *   width: number;
 * };
 *
 * type NewShape = OverrideType<Shape, string>;
 * // {
 * //   length: string;
 * //   width: string;
 * // }
 */
export type OverrideType<Type, PropertyType> = {
    [Property in keyof Pick<Type, keyof Type>]: PropertyType
};

/**
 * Make specific properties in Type be of type PropertyType,
 * while leaving the rest of the properties untouched.
 *
 * @example
 * type Shape = {
 *   length: number;
 *   width: number;
 * };
 *
 * type NewShape = OverrideTypeProps<Shape, "width", string>;
 * // {
 * //   length: number;
 * //   width: string;
 * // }
 */
export type OverrideTypeProps<Type, PropertyKey extends keyof Type, PropertyType> = Omit<Type, PropertyKey> & {
    [Property in keyof Pick<Type, PropertyKey>]: PropertyType
};

/**
 * Make all properties in Type have a specific prefix. (first letter of previous property name is capitalized)
 *
 * @example
 * type Shape = {
 *   length: number;
 *   width: number;
 * };
 *
 * type NewShape = PrefixedProps<Shape, "dimension">;
 * // {
 * //   dimensionLength: number;
 * //   dimensionWidth: number;
 * // }
 */
type Prefixed<Type, Prefix extends string> = {
    [PropertyKey in keyof Type as `${Prefix}${Capitalize<string & PropertyKey>}`]: Type[PropertyKey]
};

/**
 * Make specific properties in Type have a specific prefix. (first letter of previous property name is capitalized)
 *
 * @example
 * type Shape = {
 *   length: number;
 *   width: number;
 * };
 *
 * type NewShape = PrefixedProps<Shape, "length", "dimension">;
 * // {
 * //   dimensionLength: number;
 * //   width: number;
 * // }
 */
type PrefixedProps<Type, PrefixedPropertyKey extends keyof Type, Prefix extends string> = Omit<Type, PrefixedPropertyKey> & {
    [PropertyKey in keyof Pick<Type, PrefixedPropertyKey> as `${Prefix}${Capitalize<string & PropertyKey>}`]: Type[PropertyKey]
};

/**
 * Make all properties in Type have a specific suffix. (case is not automatically fixed)
 *
 * @example
 * type Shape = {
 *   length: number;
 *   width: number;
 * };
 *
 * type NewShape = Suffixed<Shape, "Inches">;
 * // {
 * //   lengthInches: number;
 * //   widthInches: number;
 * // }
 */
type Suffixed<Type, Suffix extends string> = {
    [PropertyKey in keyof Type as `${string & PropertyKey}${Suffix}`]: Type[PropertyKey]
};

/**
 * Make specific properties in Type have a specific suffix. (case is not automatically fixed)
 *
 * @example
 * type Shape = {
 *   length: number;
 *   width: number;
 * };
 *
 * type NewShape = SuffixedProps<Shape, "length", "Inches">;
 * // {
 * //   lengthInches: number;
 * //   width: number;
 * // }
 */
type SuffixedProps<Type, SuffixedPropertyKey extends keyof Type, Suffix extends string> = Omit<Type, SuffixedPropertyKey> & {
    [PropertyKey in keyof Pick<Type, SuffixedPropertyKey> as `${string & PropertyKey}${Suffix}`]: Type[PropertyKey]
};

/**
 * Make all properties in Type have a specific key. (merges properties into one property with the union type of old properties)
 *
 * @example
 * type Shape = {
 *   length: number;
 *   width: string;
 * };
 *
 * type NewShape = Named<Shape, "props">;
 * // {
 * //   props: number | string;
 * // }
 */
type Named<Type, PropertyName extends string> = {
    [PropertyKey in keyof Type as PropertyName]: Type[PropertyKey]
};

/**
 * Make specific properties in Type have a specific key. (merges multiple properties into one property with the union type of old properties)
 *
 * @example
 * type Shape = {
 *   length: number;
 *   width: number;
 * };
 *
 * type NewShape = NamedProps<Shape, "length", "height">;
 * // {
 * //   height: number;
 * //   width: number;
 * // }
 */
type NamedProps<Type, NamedPropertyKey extends keyof Type, PropertyName extends string> = Omit<Type, NamedPropertyKey> & {
    [PropertyKey in keyof Pick<Type, NamedPropertyKey> as PropertyName]: Type[PropertyKey]
};

/**
 * Get the union of keys from T where the properties are of type PropertyType.
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
export type KeysOfType<Type, PropertyType> = Exclude<{
    [PropertyKey in keyof Type]: Type[PropertyKey] extends PropertyType ? PropertyKey : never;
}[keyof Type], undefined>;

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
export type PropertiesOfType<Type, PropertyType> = Pick<Type, KeysOfType<Type, PropertyType>>;

// #endregion Utility Types

// #region Fluid Type Builder

export const TypeBuilder = {
    createTypeFrom<Type>(): CreateTypeFromBuilder<Type> {
        return createTypeFromBuilder<Type>();
    }
} as const;

type CreateTypeFromBuilder<Type> = {
    /** Make all properties in Type… */
    makeAllProperties(): AllPropertiesPropertyBuilder<Type>;
    /** Make properties with keys in Type… */
    makeProperties<PropertyKey extends keyof Type>(..._propertyKeys: PropertyKey[]): PropertiesWithKeysPropertyBuilder<Type, PropertyKey>;
    /** Make properties of type PropertyType in Type… */
    makePropertiesOfType<PropertyType>(): PropertiesOfTypePropertyBuilder<Type, PropertyType>;
};

type CreateTypeFromMoreBuilder<Type, LastBuilder> = {
    /**
     * Gets an `undefined` value shaped like type T.
     *
     * Use this property with `typeof` to get this builder's type.
     *
     * @example
     * type Shape = {
     *   length?: number;
     *   width?: number;
     * };
     * const newShape = TypeBuilder.fromType<Shape>().makeAllProperties().required().build;
     * const NewShape = typeof newShape;
     * // {
     * //   length: number;
     * //   width: number;
     * // }
     */
    readonly build: Type;
    /** Allows chaining more modifications to the last builder. */
    readonly and: LastBuilder;
    /** Make all properties in Type… */
    makeAllProperties(): AllPropertiesPropertyBuilder<Type>;
    /** Make properties with keys in Type… */
    makeProperties<PropertyKey extends keyof Type>(..._propertyKeys: PropertyKey[]): PropertiesWithKeysPropertyBuilder<Type, PropertyKey>;
    /** Make properties of type PropertyType in Type… */
    makePropertiesOfType<PropertyType>(): PropertiesOfTypePropertyBuilder<Type, PropertyType>;
};

type AllPropertiesPropertyBuilder<Type> = {
    /**
     * Make all properties in Type able to be set to null.
     *
     * @example
     * type Shape = {
     *   length: number;
     *   width: number;
     * };
     *
     * type NewShape = Nullable<Shape>;
     * // {
     * //   length: number | null;
     * //   width: number | null;
     * // }
     */
    nullable(): CreateTypeFromMoreBuilder<Nullable<Type>, AllPropertiesPropertyBuilder<Nullable<Type>>>;
    /**
     * Make properties in Type not able to be set to null.
     *
     * @example
     * type Shape = {
     *   length?: number | null;
     *   width?: number | null;
     * };
     *
     * type NewShape = NotNullable<Shape>;
     * // {
     * //   length?: number;
     * //   width?: number;
     * // }
     */
    notNullable(): CreateTypeFromMoreBuilder<NotNullable<Type>, AllPropertiesPropertyBuilder<NotNullable<Type>>>;
    /**
     * Make all properties in Type able to be set to undefined.
     *
     * @example
     * type Shape = {
     *   length: number | null;
     *   width: number | null;
     * };
     *
     * type NewShape = Undefinable<Shape>;
     * // {
     * //   length: number | null | undefined;
     * //   width: number | null | undefined;
     * // }
     */
    undefined(): CreateTypeFromMoreBuilder<Undefinable<Type>, AllPropertiesPropertyBuilder<Undefinable<Type>>>;
    /**
     * Make all properties in Type not able to be set to undefined. (does nothing to optional properties)
     *
     * @example
     * type Shape = {
     *   length: number | undefined;
     *   width?: number | undefined;
     * };
     *
     * type NewShape = NotUndefinable<Shape>;
     * // {
     * //   length: number;
     * //   width?: number | undefined;
     * // }
     */
    notUndefined(): CreateTypeFromMoreBuilder<NotUndefinable<Type>, AllPropertiesPropertyBuilder<NotUndefinable<Type>>>;
    /**
     * Make all properties in Type optional.
     *
     * @example
     * type Shape = {
     *   length: number;
     *   width: number;
     * };
     *
     * type NewShape = Partial<Shape>;
     * // {
     * //   length?: number | undefined;
     * //   width?: number | undefined;
     * // }
     */
    optional(): CreateTypeFromMoreBuilder<Partial<Type>, AllPropertiesPropertyBuilder<Partial<Type>>>;
    /**
     * Make all properties in Type required. (optional properties will become required and cannot be set to undefined)
     *
     * @example
     * type Shape = {
     *   length?: number | undefined;
     *   width?: number | undefined;
     * };
     *
     * type NewShape = Required<Shape>;
     * // {
     * //   length: number;
     * //   width: number;
     * // }
     */
    required(): CreateTypeFromMoreBuilder<Required<Type>, AllPropertiesPropertyBuilder<Required<Type>>>;
    /**
     * Make all properties in Type be of type PropertyType.
     *
     * @example
     * type Shape = {
     *   length: number;
     *   width: number;
     * };
     *
     * type NewShape = OverrideType<Shape, string>;
     * // {
     * //   length: string;
     * //   width: string;
     * // }
     */
    typed<PropertyType>(): CreateTypeFromMoreBuilder<OverrideType<Type, PropertyType>, AllPropertiesPropertyBuilder<OverrideType<Type, PropertyType>>>;
    /**
     * Make all properties in Type unable to be set to null or undefined. (optional properties can still be set to undefined)
     *
     * @example
     * type Shape = {
     *   length: number | null | undefined;
     *   width?: number | null | undefined;
     * };
     *
     * type NewShape = NonNullable<Shape>;
     * // {
     * //   length: number;
     * //   width?: number | undefined;
     * // }
     */
    defined(): CreateTypeFromMoreBuilder<NonNullable<Type>, AllPropertiesPropertyBuilder<NonNullable<Type>>>;
    /**
     * Make all properties in Type have a specific prefix. (first letter of previous property name is capitalized)
     *
     * @example
     * type Shape = {
     *   length: number;
     *   width: number;
     * };
     *
     * type NewShape = PrefixedProps<Shape, "dimension">;
     * // {
     * //   dimensionLength: number;
     * //   dimensionWidth: number;
     * // }
     */
    prefixed<Prefix extends string>(_prefix: Prefix): CreateTypeFromMoreBuilder<Prefixed<Type, Prefix>, AllPropertiesPropertyBuilder<Prefixed<Type, Prefix>>>;
    /**
     * Make all properties in Type have a specific suffix. (case is not automatically fixed)
     *
     * @example
     * type Shape = {
     *   length: number;
     *   width: number;
     * };
     *
     * type NewShape = Suffixed<Shape, "Inches">;
     * // {
     * //   lengthInches: number;
     * //   widthInches: number;
     * // }
     */
    suffixed<Suffix extends string>(_suffix: Suffix): CreateTypeFromMoreBuilder<Suffixed<Type, Suffix>, AllPropertiesPropertyBuilder<Suffixed<Type, Suffix>>>;
    /**
     * Make all properties in Type have a specific key. (merges properties into one property with the union type of old properties)
     *
     * @example
     * type Shape = {
     *   length: number;
     *   width: string;
     * };
     *
     * type NewShape = Named<Shape, "props">;
     * // {
     * //   props: number | string;
     * // }
     */
    named<PropertyName extends string>(_propertyName: PropertyName): CreateTypeFromMoreBuilder<Named<Type, PropertyName>, AllPropertiesPropertyBuilder<Named<Type, PropertyName>>>;
};

type PropertiesWithKeysPropertyBuilder<Type, PropertyKey extends keyof Type> = {
    /**
     * Make specific properties in Type able to be set to null,
     * while leaving the rest of the properties untouched.
     *
     * @example
     * type Shape = {
     *   length: number;
     *   width?: number | undefined;
     * };
     *
     * type NewShape = NullableProps<Shape, "width">;
     * // {
     * //   length: number;
     * //   width?: number | null | undefined;
     * // }
     */
    nullable(): CreateTypeFromMoreBuilder<NullableProps<Type, PropertyKey>, PropertiesWithKeysPropertyBuilder<NullableProps<Type, PropertyKey>, PropertyKey>>;
    /**
     * Make specific properties in Type not able to be set to null,
     * while leaving the rest of the properties untouched.
     *
     * @example
     * type Shape = {
     *   length?: number | null | undefined;
     *   width?: number | null | undefined;
     * };
     *
     * type NewShape = NotNullableProps<Shape, "length">;
     * // {
     * //   length?: number;
     * //   width?: number | null | undefined;
     * // }
     */
    notNullable(): CreateTypeFromMoreBuilder<NotNullableProps<Type, PropertyKey>, PropertiesWithKeysPropertyBuilder<NotNullableProps<Type, PropertyKey>, PropertyKey>>;
    /**
     * Make specific properties in Type able to be set to undefined.
     *
     * @example
     * type Shape = {
     *   length: number | null;
     *   width: number | null;
     * };
     *
     * type NewShape = UndefinableProps<Shape, "length">;
     * // {
     * //   length: number | null | undefined;
     * //   width: number | null;
     * // }
     */
    undefined(): CreateTypeFromMoreBuilder<UndefinableProps<Type, PropertyKey>, PropertiesWithKeysPropertyBuilder<UndefinableProps<Type, PropertyKey>, PropertyKey>>;
    /**
     * Make specific properties in Type not able to be set to undefined. (does nothing to optional properties)
     *
     * @example
     * type Shape = {
     *   length: number | undefined;
     *   width?: number | undefined;
     * };
     *
     * type NewShape = NotUndefinableProps<Shape, "length">;
     * // {
     * //   length: number;
     * //   width?: number | undefined;
     * // }
     */
    notUndefined(): CreateTypeFromMoreBuilder<NotUndefinableProps<Type, PropertyKey>, PropertiesWithKeysPropertyBuilder<NotUndefinableProps<Type, PropertyKey>, PropertyKey>>;
    /**
     * Make specific properties in Type optional,
     * while leaving the rest of the properties untouched.
     *
     * @example
     * type Shape = {
     *   length: number;
     *   width: number;
     * };
     *
     * type NewShape = OptionalProps<Shape, "width">;
     * // {
     * //   length: number;
     * //   width?: number | undefined;
     * // }
     */
    optional(): CreateTypeFromMoreBuilder<OptionalProps<Type, PropertyKey>, PropertiesWithKeysPropertyBuilder<OptionalProps<Type, PropertyKey>, PropertyKey>>;
    /**
     * Make specific properties in Type required,
     * while leaving the rest of the properties untouched. (optional properties will become required and cannot be set to undefined)
     *
     * @example
     * type Shape = {
     *   length?: number | null | undefined;
     *   width?: number | null | undefined;
     * };
     *
     * type NewShape = RequiredProps<Shape, "length">;
     * // {
     * //   length: number | null;
     * //   width?: number | null | undefined;
     * // }
     */
    required(): CreateTypeFromMoreBuilder<RequiredProps<Type, PropertyKey>, PropertiesWithKeysPropertyBuilder<RequiredProps<Type, PropertyKey>, PropertyKey>>;
    /**
     * Make specific properties in Type be of type PropertyType,
     * while leaving the rest of the properties untouched.
     *
     * @example
     * type Shape = {
     *   length: number;
     *   width: number;
     * };
     *
     * type NewShape = OverrideTypeProps<Shape, "width", string>;
     * // {
     * //   length: number;
     * //   width: string;
     * // }
     */
    typed<PropertyType>(): CreateTypeFromMoreBuilder<OverrideTypeProps<Type, PropertyKey, PropertyType>, PropertiesWithKeysPropertyBuilder<OverrideTypeProps<Type, PropertyKey, PropertyType>, PropertyKey>>;
    /**
     * Make specific properties in Type unable to be set to null or undefined,
     * while leaving the rest of the properties untouched. (optional properties can still be set to undefined)
     *
     * @example
     * type Shape = {
     *   length: number | null | undefined;
     *   width?: number | null | undefined;
     * };
     *
     * type NewShape = DefinedProps<Shape, "length" | "width">;
     * // {
     * //   length: number;
     * //   width?: number | undefined;
     * // }
     */
    defined(): CreateTypeFromMoreBuilder<DefinedProps<Type, PropertyKey>, PropertiesWithKeysPropertyBuilder<DefinedProps<Type, PropertyKey>, PropertyKey>>;
    /**
     * Make specific properties in Type have a specific prefix. (first letter of previous property name is capitalized)
     *
     * @example
     * type Shape = {
     *   length: number;
     *   width: number;
     * };
     *
     * type NewShape = PrefixedProps<Shape, "length", "dimension">;
     * // {
     * //   dimensionLength: number;
     * //   width: number;
     * // }
     */
    prefixed<Prefix extends string>(_prefix: Prefix): CreateTypeFromMoreBuilder<PrefixedProps<Type, PropertyKey, Prefix>, PropertiesWithKeysPropertyBuilder<PrefixedProps<Type, PropertyKey, Prefix>, Exclude<keyof PrefixedProps<Type, PropertyKey, Prefix>, keyof Type>>>;
    /**
     * Make specific properties in Type have a specific suffix. (case is not automatically fixed)
     *
     * @example
     * type Shape = {
     *   length: number;
     *   width: number;
     * };
     *
     * type NewShape = SuffixedProps<Shape, "length", "Inches">;
     * // {
     * //   lengthInches: number;
     * //   width: number;
     * // }
     */
    suffixed<Suffix extends string>(_suffix: Suffix): CreateTypeFromMoreBuilder<SuffixedProps<Type, PropertyKey, Suffix>, PropertiesWithKeysPropertyBuilder<SuffixedProps<Type, PropertyKey, Suffix>, Exclude<keyof SuffixedProps<Type, PropertyKey, Suffix>, keyof Type>>>;
    /**
     * Make specific properties in Type have a specific key. (merges multiple properties into one property with the union type of old properties)
     *
     * @example
     * type Shape = {
     *   length: number;
     *   width: number;
     * };
     *
     * type NewShape = NamedProps<Shape, "length", "height">;
     * // {
     * //   height: number;
     * //   width: number;
     * // }
     */
    named<PropertyName extends string>(_propertyName: PropertyName): CreateTypeFromMoreBuilder<NamedProps<Type, PropertyKey, PropertyName>, PropertiesWithKeysPropertyBuilder<NamedProps<Type, PropertyKey, PropertyName>, Exclude<keyof NamedProps<Type, PropertyKey, PropertyName>, keyof Type>>>;
};

type PropertiesOfTypePropertyBuilder<Type, PropertyType> = {
    /**
     * Make properties of type PropertyType in Type able to be set to null,
     * while leaving the rest of the properties untouched.
     *
     * @example
     * type Shape = {
     *   length: number;
     *   width?: number | undefined;
     * };
     *
     * type NewShape = NullableProps<Shape, keyof PropertiesOfType<Shape, number>>;
     * // {
     * //   length: number | null;
     * //   width?: number | null | undefined;
     * // }
     */
    nullable(): CreateTypeFromMoreBuilder<NullableProps<Type, keyof PropertiesOfType<Type, PropertyType>>, PropertiesOfTypePropertyBuilder<NullableProps<Type, keyof PropertiesOfType<Type, PropertyType>>, PropertyType>>;
    /**
     * Make properties of type PropertyType in Type not able to be set to null,
     * while leaving the rest of the properties untouched.
     *
     * @example
     * type Shape = {
     *   length: number | null;
     *   width?: number | null | undefined;
     * };
     *
     * type NewShape = NotNullableProps<Shape, keyof PropertiesOfType<Shape, number>>;
     * // {
     * //   length: number;
     * //   width?: number | undefined;
     * // }
     */
    notNullable(): CreateTypeFromMoreBuilder<NotNullableProps<Type, keyof PropertiesOfType<Type, PropertyType>>, PropertiesOfTypePropertyBuilder<NotNullableProps<Type, keyof PropertiesOfType<Type, PropertyType>>, PropertyType>>;
    /**
     * Make properties of type PropertyType in Type able to be set to undefined.
     *
     * @example
     * type Shape = {
     *   length: number | null;
     *   width: number | null;
     * };
     *
     * type NewShape = UndefinableProps<Shape, keyof PropertiesOfType<Shape, number>>;
     * // {
     * //   length: number | null | undefined;
     * //   width: number | null | undefined;
     * // }
     */
    undefined(): CreateTypeFromMoreBuilder<UndefinableProps<Type, keyof PropertiesOfType<Type, PropertyType>>, PropertiesOfTypePropertyBuilder<UndefinableProps<Type, keyof PropertiesOfType<Type, PropertyType>>, PropertyType>>;
    /**
     * Make properties of type PropertyType in Type not able to be set to undefined. (does nothing to optional properties)
     *
     * @example
     * type Shape = {
     *   length: number | undefined;
     *   width?: number | undefined;
     * };
     *
     * type NewShape = NotUndefinableProps<Shape, keyof PropertiesOfType<Shape, number>>;
     * // {
     * //   length: number;
     * //   width?: number | undefined;
     * // }
     */
    notUndefined(): CreateTypeFromMoreBuilder<NotUndefinableProps<Type, keyof PropertiesOfType<Type, PropertyType>>, PropertiesOfTypePropertyBuilder<NotUndefinableProps<Type, keyof PropertiesOfType<Type, PropertyType>>, PropertyType>>;
    /**
     * Make properties of type PropertyType in Type optional,
     * while leaving the rest of the properties untouched.
     *
     * @example
     * type Shape = {
     *   length: number;
     *   width: number;
     * };
     *
     * type NewShape = OptionalProps<Shape, keyof PropertiesOfType<Shape, number>>;
     * // {
     * //   length?: number | undefined;
     * //   width?: number | undefined;
     * // }
     */
    optional(): CreateTypeFromMoreBuilder<OptionalProps<Type, keyof PropertiesOfType<Type, PropertyType>>, PropertiesOfTypePropertyBuilder<OptionalProps<Type, keyof PropertiesOfType<Type, PropertyType>>, PropertyType>>;
    /**
     * Make properties of type PropertyType in Type required,
     * while leaving the rest of the properties untouched. (optional properties will become required and cannot be set to undefined)
     *
     * @example
     * type Shape = {
     *   length: number | null | undefined;
     *   width?: number | null | undefined;
     * };
     *
     * type NewShape = RequiredProps<Shape, keyof PropertiesOfType<Shape, number>>;
     * // {
     * //   length: number | null | undefined;
     * //   width: number | null;
     * // }
     */
    required(): CreateTypeFromMoreBuilder<RequiredProps<Type, keyof PropertiesOfType<Type, PropertyType>>, PropertiesOfTypePropertyBuilder<RequiredProps<Type, keyof PropertiesOfType<Type, PropertyType>>, PropertyType>>;
    /**
     * Make properties of type PropertyType in Type be of type NewPropertyType,
     * while leaving the rest of the properties untouched.
     *
     * @example
     * type Shape = {
     *   length: number;
     *   width: number;
     * };
     *
     * type NewShape = OverrideTypeProps<Shape, keyof PropertiesOfType<Shape, number>, string>;
     * // {
     * //   length: string;
     * //   width: string;
     * // }
     */
    typed<NewPropertyType>(): CreateTypeFromMoreBuilder<OverrideTypeProps<Type, keyof PropertiesOfType<Type, PropertyType>, NewPropertyType>, PropertiesOfTypePropertyBuilder<OverrideTypeProps<Type, keyof PropertiesOfType<Type, PropertyType>, NewPropertyType>, NewPropertyType>>;
    /**
     * Make properties of type PropertyType in Type unable to be set to null or undefined,
     * while leaving the rest of the properties untouched. (optional properties can still be set to undefined)
     *
     * @example
     * type Shape = {
     *   length: number | null | undefined;
     *   width?: number | null | undefined;
     * };
     *
     * type NewShape = DefinedProps<Shape, keyof PropertiesOfType<Shape, number>>;
     * // {
     * //   length: number;
     * //   width?: number | undefined;
     * // }
     */
    defined(): CreateTypeFromMoreBuilder<DefinedProps<Type, keyof PropertiesOfType<Type, PropertyType>>, PropertiesOfTypePropertyBuilder<DefinedProps<Type, keyof PropertiesOfType<Type, PropertyType>>, PropertyType>>;
    /**
     * Make properties of type PropertyType in Type have a specific prefix. (first letter of previous property name is capitalized)
     *
     * @example
     * type Shape = {
     *   length: number;
     *   width: number;
     * };
     *
     * type NewShape = PrefixedProps<Shape, keyof PropertiesOfType<Shape, number>, "dimension">;
     * // {
     * //   dimensionLength: number;
     * //   dimensionWidth: number;
     * // }
     */
    prefixed<Prefix extends string>(_prefix: Prefix): CreateTypeFromMoreBuilder<PrefixedProps<Type, keyof PropertiesOfType<Type, PropertyType>, Prefix>, PropertiesOfTypePropertyBuilder<PrefixedProps<Type, keyof PropertiesOfType<Type, PropertyType>, Prefix>, PropertyType>>;
    /**
     * Make properties of type PropertyType in Type have a specific suffix. (case is not automatically fixed)
     *
     * @example
     * type Shape = {
     *   length: number;
     *   width: number;
     * };
     *
     * type NewShape = SuffixedProps<Shape, keyof PropertiesOfType<Shape, number>, "Inches">;
     * // {
     * //   lengthInches: number;
     * //   widthInches: number;
     * // }
     */
    suffixed<Suffix extends string>(_suffix: Suffix): CreateTypeFromMoreBuilder<SuffixedProps<Type, keyof PropertiesOfType<Type, PropertyType>, Suffix>, PropertiesOfTypePropertyBuilder<SuffixedProps<Type, keyof PropertiesOfType<Type, PropertyType>, Suffix>, PropertyType>>;
    /**
     * Make properties of type PropertyType in Type have a specific key. (merges multiple properties into one property with the union type of old properties)
     *
     * @example
     * type Square = {
     *   length: number;
     *   width: number;
     * };
     *
     * type NewShape = NamedProps<Square, keyof PropertiesOfType<Square, number>, "side">;
     * // {
     * //   side: number;
     * // }
     */
    named<PropertyName extends string>(_propertyName: PropertyName): CreateTypeFromMoreBuilder<NamedProps<Type, keyof PropertiesOfType<Type, PropertyType>, PropertyName>, PropertiesOfTypePropertyBuilder<NamedProps<Type, keyof PropertiesOfType<Type, PropertyType>, PropertyName>, PropertyType>>;
};

function createAllPropertiesPropertyBuilder<Type>(): AllPropertiesPropertyBuilder<Type> {
    function transformTo<NewType>(): CreateTypeFromMoreBuilder<NewType, AllPropertiesPropertyBuilder<NewType>> {
        return createTypeFromMoreBuilder<NewType, AllPropertiesPropertyBuilder<NewType>>(createAllPropertiesPropertyBuilder<NewType>());
    }

    return {
        nullable(): CreateTypeFromMoreBuilder<Nullable<Type>, AllPropertiesPropertyBuilder<Nullable<Type>>> {
            return transformTo<Nullable<Type>>();
        },
        notNullable(): CreateTypeFromMoreBuilder<NotNullable<Type>, AllPropertiesPropertyBuilder<NotNullable<Type>>> {
            return transformTo<NotNullable<Type>>();
        },
        undefined(): CreateTypeFromMoreBuilder<Undefinable<Type>, AllPropertiesPropertyBuilder<Undefinable<Type>>> {
            return transformTo<Undefinable<Type>>();
        },
        notUndefined(): CreateTypeFromMoreBuilder<NotUndefinable<Type>, AllPropertiesPropertyBuilder<NotUndefinable<Type>>> {
            return transformTo<NotUndefinable<Type>>();
        },
        optional(): CreateTypeFromMoreBuilder<Partial<Type>, AllPropertiesPropertyBuilder<Partial<Type>>> {
            return transformTo<Partial<Type>>();
        },
        required(): CreateTypeFromMoreBuilder<Required<Type>, AllPropertiesPropertyBuilder<Required<Type>>> {
            return transformTo<Required<Type>>();
        },
        typed<PropertyType>(): CreateTypeFromMoreBuilder<OverrideType<Type, PropertyType>, AllPropertiesPropertyBuilder<OverrideType<Type, PropertyType>>> {
            return transformTo<OverrideType<Type, PropertyType>>();
        },
        defined(): CreateTypeFromMoreBuilder<NonNullable<Type>, AllPropertiesPropertyBuilder<NonNullable<Type>>> {
            return transformTo<NonNullable<Type>>();
        },
        prefixed<Prefix extends string>(_prefix: Prefix): CreateTypeFromMoreBuilder<Prefixed<Type, Prefix>, AllPropertiesPropertyBuilder<Prefixed<Type, Prefix>>> {
            return transformTo<Prefixed<Type, Prefix>>();
        },
        suffixed<Suffix extends string>(_suffix: Suffix): CreateTypeFromMoreBuilder<Suffixed<Type, Suffix>, AllPropertiesPropertyBuilder<Suffixed<Type, Suffix>>> {
            return transformTo<Suffixed<Type, Suffix>>();
        },
        named<PropertyName extends string>(_propertyName: PropertyName): CreateTypeFromMoreBuilder<Named<Type, PropertyName>, AllPropertiesPropertyBuilder<Named<Type, PropertyName>>> {
            return transformTo<Named<Type, PropertyName>>();
        }
    };
}

function createPropertiesWithKeysPropertyBuilder<Type, PropertyKey extends keyof Type>(): PropertiesWithKeysPropertyBuilder<Type, PropertyKey> {
    function transformTo<NewType, NewPropertyKey extends keyof NewType>(): CreateTypeFromMoreBuilder<NewType, PropertiesWithKeysPropertyBuilder<NewType, NewPropertyKey>> {
        return createTypeFromMoreBuilder<NewType, PropertiesWithKeysPropertyBuilder<NewType, NewPropertyKey>>(createPropertiesWithKeysPropertyBuilder<NewType, NewPropertyKey>());
    }

    return {
        nullable(): CreateTypeFromMoreBuilder<NullableProps<Type, PropertyKey>, PropertiesWithKeysPropertyBuilder<NullableProps<Type, PropertyKey>, PropertyKey>> {
            return transformTo<NullableProps<Type, PropertyKey>, PropertyKey>();
        },
        notNullable(): CreateTypeFromMoreBuilder<NotNullableProps<Type, PropertyKey>, PropertiesWithKeysPropertyBuilder<NotNullableProps<Type, PropertyKey>, PropertyKey>> {
            return transformTo<NotNullableProps<Type, PropertyKey>, PropertyKey>();
        },
        undefined(): CreateTypeFromMoreBuilder<UndefinableProps<Type, PropertyKey>, PropertiesWithKeysPropertyBuilder<UndefinableProps<Type, PropertyKey>, PropertyKey>> {
            return transformTo<UndefinableProps<Type, PropertyKey>, PropertyKey>();
        },
        notUndefined(): CreateTypeFromMoreBuilder<NotUndefinableProps<Type, PropertyKey>, PropertiesWithKeysPropertyBuilder<NotUndefinableProps<Type, PropertyKey>, PropertyKey>> {
            return transformTo<NotUndefinableProps<Type, PropertyKey>, PropertyKey>();
        },
        optional(): CreateTypeFromMoreBuilder<OptionalProps<Type, PropertyKey>, PropertiesWithKeysPropertyBuilder<OptionalProps<Type, PropertyKey>, PropertyKey>> {
            return transformTo<OptionalProps<Type, PropertyKey>, PropertyKey>();
        },
        required(): CreateTypeFromMoreBuilder<RequiredProps<Type, PropertyKey>, PropertiesWithKeysPropertyBuilder<RequiredProps<Type, PropertyKey>, PropertyKey>> {
            return transformTo<RequiredProps<Type, PropertyKey>, PropertyKey>();
        },
        typed<PropertyType>(): CreateTypeFromMoreBuilder<OverrideTypeProps<Type, PropertyKey, PropertyType>, PropertiesWithKeysPropertyBuilder<OverrideTypeProps<Type, PropertyKey, PropertyType>, PropertyKey>> {
            return transformTo<OverrideTypeProps<Type, PropertyKey, PropertyType>, PropertyKey>();
        },
        defined(): CreateTypeFromMoreBuilder<DefinedProps<Type, PropertyKey>, PropertiesWithKeysPropertyBuilder<DefinedProps<Type, PropertyKey>, PropertyKey>> {
            return transformTo<DefinedProps<Type, PropertyKey>, PropertyKey>();
        },
        prefixed<Prefix extends string>(_prefix: Prefix): CreateTypeFromMoreBuilder<PrefixedProps<Type, PropertyKey, Prefix>, PropertiesWithKeysPropertyBuilder<PrefixedProps<Type, PropertyKey, Prefix>, Exclude<keyof PrefixedProps<Type, PropertyKey, Prefix>, keyof Type>>> {
            return transformTo<PrefixedProps<Type, PropertyKey, Prefix>, Exclude<keyof PrefixedProps<Type, PropertyKey, Prefix>, keyof Type>>();
        },
        suffixed<Suffix extends string>(_suffix: Suffix): CreateTypeFromMoreBuilder<SuffixedProps<Type, PropertyKey, Suffix>, PropertiesWithKeysPropertyBuilder<SuffixedProps<Type, PropertyKey, Suffix>, Exclude<keyof SuffixedProps<Type, PropertyKey, Suffix>, keyof Type>>> {
            return transformTo<SuffixedProps<Type, PropertyKey, Suffix>, Exclude<keyof SuffixedProps<Type, PropertyKey, Suffix>, keyof Type>>();
        },
        named<PropertyName extends string>(_propertyName: PropertyName): CreateTypeFromMoreBuilder<NamedProps<Type, PropertyKey, PropertyName>, PropertiesWithKeysPropertyBuilder<NamedProps<Type, PropertyKey, PropertyName>, Exclude<keyof NamedProps<Type, PropertyKey, PropertyName>, keyof Type>>> {
            return transformTo<NamedProps<Type, PropertyKey, PropertyName>, Exclude<keyof NamedProps<Type, PropertyKey, PropertyName>, keyof Type>>();
        }
    };
}

function createPropertiesOfTypePropertyBuilder<Type, PropertyType>(): PropertiesOfTypePropertyBuilder<Type, PropertyType> {
    function transformTo<NewType>(): CreateTypeFromMoreBuilder<NewType, PropertiesOfTypePropertyBuilder<NewType, PropertyType>> {
        return createTypeFromMoreBuilder<NewType, PropertiesOfTypePropertyBuilder<NewType, PropertyType>>(createPropertiesOfTypePropertyBuilder<NewType, PropertyType>());
    }

    return {
        nullable(): CreateTypeFromMoreBuilder<NullableProps<Type, keyof PropertiesOfType<Type, PropertyType>>, PropertiesOfTypePropertyBuilder<NullableProps<Type, keyof PropertiesOfType<Type, PropertyType>>, PropertyType>> {
            return transformTo<NullableProps<Type, keyof PropertiesOfType<Type, PropertyType>>>();
        },
        notNullable(): CreateTypeFromMoreBuilder<NotNullableProps<Type, keyof PropertiesOfType<Type, PropertyType>>, PropertiesOfTypePropertyBuilder<NotNullableProps<Type, keyof PropertiesOfType<Type, PropertyType>>, PropertyType>> {
            return transformTo<NotNullableProps<Type, keyof PropertiesOfType<Type, PropertyType>>>();
        },
        undefined(): CreateTypeFromMoreBuilder<UndefinableProps<Type, keyof PropertiesOfType<Type, PropertyType>>, PropertiesOfTypePropertyBuilder<UndefinableProps<Type, keyof PropertiesOfType<Type, PropertyType>>, PropertyType>> {
            return transformTo<UndefinableProps<Type, keyof PropertiesOfType<Type, PropertyType>>>();
        },
        notUndefined(): CreateTypeFromMoreBuilder<NotUndefinableProps<Type, keyof PropertiesOfType<Type, PropertyType>>, PropertiesOfTypePropertyBuilder<NotUndefinableProps<Type, keyof PropertiesOfType<Type, PropertyType>>, PropertyType>> {
            return transformTo<NotUndefinableProps<Type, keyof PropertiesOfType<Type, PropertyType>>>();
        },
        optional(): CreateTypeFromMoreBuilder<OptionalProps<Type, keyof PropertiesOfType<Type, PropertyType>>, PropertiesOfTypePropertyBuilder<OptionalProps<Type, keyof PropertiesOfType<Type, PropertyType>>, PropertyType>> {
            return transformTo<OptionalProps<Type, keyof PropertiesOfType<Type, PropertyType>>>();
        },
        required(): CreateTypeFromMoreBuilder<RequiredProps<Type, keyof PropertiesOfType<Type, PropertyType>>, PropertiesOfTypePropertyBuilder<RequiredProps<Type, keyof PropertiesOfType<Type, PropertyType>>, PropertyType>> {
            return transformTo<RequiredProps<Type, keyof PropertiesOfType<Type, PropertyType>>>();
        },
        typed<NewPropertyType>(): CreateTypeFromMoreBuilder<OverrideTypeProps<Type, keyof PropertiesOfType<Type, PropertyType>, NewPropertyType>, PropertiesOfTypePropertyBuilder<OverrideTypeProps<Type, keyof PropertiesOfType<Type, PropertyType>, NewPropertyType>, NewPropertyType>> {
            return transformTo<OverrideTypeProps<Type, keyof PropertiesOfType<Type, PropertyType>, NewPropertyType>>();
        },
        defined(): CreateTypeFromMoreBuilder<DefinedProps<Type, keyof PropertiesOfType<Type, PropertyType>>, PropertiesOfTypePropertyBuilder<DefinedProps<Type, keyof PropertiesOfType<Type, PropertyType>>, PropertyType>> {
            return transformTo<DefinedProps<Type, keyof PropertiesOfType<Type, PropertyType>>>();
        },
        prefixed<Prefix extends string>(_prefix: Prefix): CreateTypeFromMoreBuilder<PrefixedProps<Type, keyof PropertiesOfType<Type, PropertyType>, Prefix>, PropertiesOfTypePropertyBuilder<PrefixedProps<Type, keyof PropertiesOfType<Type, PropertyType>, Prefix>, PropertyType>> {
            return transformTo<PrefixedProps<Type, keyof PropertiesOfType<Type, PropertyType>, Prefix>>();
        },
        suffixed<Suffix extends string>(_suffix: Suffix): CreateTypeFromMoreBuilder<SuffixedProps<Type, keyof PropertiesOfType<Type, PropertyType>, Suffix>, PropertiesOfTypePropertyBuilder<SuffixedProps<Type, keyof PropertiesOfType<Type, PropertyType>, Suffix>, PropertyType>> {
            return transformTo<SuffixedProps<Type, keyof PropertiesOfType<Type, PropertyType>, Suffix>>();
        },
        named<PropertyName extends string>(_propertyName: PropertyName): CreateTypeFromMoreBuilder<NamedProps<Type, keyof PropertiesOfType<Type, PropertyType>, PropertyName>, PropertiesOfTypePropertyBuilder<NamedProps<Type, keyof PropertiesOfType<Type, PropertyType>, PropertyName>, PropertyType>> {
            return transformTo<NamedProps<Type, keyof PropertiesOfType<Type, PropertyType>, PropertyName>>();
        }
    };
}

function createTypeFromBuilder<Type>(): CreateTypeFromBuilder<Type> {
    return {
        makeAllProperties(): AllPropertiesPropertyBuilder<Type> {
            return createAllPropertiesPropertyBuilder<Type>();
        },
        makeProperties<PropertyKey extends keyof Type>(..._propertyKeys: PropertyKey[]): PropertiesWithKeysPropertyBuilder<Type, PropertyKey> {
            return createPropertiesWithKeysPropertyBuilder<Type, PropertyKey>();
        },
        makePropertiesOfType<PropertyType>(): PropertiesOfTypePropertyBuilder<Type, PropertyType> {
            return createPropertiesOfTypePropertyBuilder<Type, PropertyType>();
        }
    };
}

function createTypeFromMoreBuilder<Type, LastBuilder>(lastBuilder: LastBuilder): CreateTypeFromMoreBuilder<Type, LastBuilder> {
    return {
        build: undefined as unknown as Type,
        and: lastBuilder,
        makeAllProperties(): AllPropertiesPropertyBuilder<Type> {
            return createAllPropertiesPropertyBuilder<Type>();
        },
        makeProperties<PropertyKey extends keyof Type>(..._propertyKeys: PropertyKey[]): PropertiesWithKeysPropertyBuilder<Type, PropertyKey> {
            return createPropertiesWithKeysPropertyBuilder<Type, PropertyKey>();
        },
        makePropertiesOfType<PropertyType>(): PropertiesOfTypePropertyBuilder<Type, PropertyType> {
            return createPropertiesOfTypePropertyBuilder<Type, PropertyType>();
        }
    };
}

// #endregion Fluid Type Builder