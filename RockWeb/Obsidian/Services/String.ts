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
 * Is the value an empty string?
 * @param val
 */
export function isEmpty( val: unknown )
{
    if ( typeof val === 'string' )
    {
        return val.length === 0;
    }

    return false;
}

/**
 * Is the value an empty string?
 * @param val
 */
export function isWhitespace( val: unknown )
{
    if ( typeof val === 'string' )
    {
        return val.trim().length === 0;
    }

    return false;
}

/**
 * Is the value null, undefined or whitespace?
 * @param val
 */
export function isNullOrWhitespace( val: unknown )
{
    return isWhitespace( val ) || val === undefined || val === null;
}

/**
 * Turns "MyCamelCaseString" into "My Camel Case String"
 * @param val
 */
export function splitCamelCase( val: unknown )
{
    if ( typeof val === 'string' )
    {
        return val.replace( /([a-z])([A-Z])/g, '$1 $2' );
    }

    return val;
}

/**
 * Returns an English comma-and fragment.
 * Ex: ['a', 'b', 'c'] => 'a, b, and c'
 * @param strs
 */
export function asCommaAnd( strs: string[] )
{
    if ( strs.length === 0 )
    {
        return '';
    }

    if ( strs.length === 1 )
    {
        return strs[ 0 ];
    }

    if ( strs.length === 2 )
    {
        return `${strs[ 0 ]} and ${strs[ 1 ]}`;
    }

    const last = strs.pop();
    return `${strs.join( ', ' )}, and ${last}`;
}

/**
 * Convert the string to the title case.
 * hellO worlD => Hello World
 * @param str
 */
export function toTitleCase( str: string | null )
{
    if ( !str )
    {
        return '';
    }

    return str.replace( /\w\S*/g, ( word ) =>
    {
        return word.charAt( 0 ).toUpperCase() + word.substr( 1 ).toLowerCase();
    } );
}

/**
 * Returns a singular or plural phrase depending on if the number is 1.
 * (0, Cat, Cats) => Cats
 * (1, Cat, Cats) => Cat
 * (2, Cat, Cats) => Cats
 * @param num
 * @param singular
 * @param plural
 */
export function pluralConditional( num: number, singular: string, plural: string )
{
    return num === 1 ? singular : plural;
}

/**
 * Formats as a phone number
 * 3214567 => 321-4567
 * 3214567890 => (321) 456-7890
 * @param str
 */
export function formatPhoneNumber ( str: string )
{
    str = stripPhoneNumber( str );

    if ( str.length === 7 )
    {
        return `${str.substring( 0, 3 )}-${str.substring( 3, 7 )}`;
    }

    if ( str.length === 10 )
    {
        return `(${str.substring( 0, 3 )}) ${str.substring( 3, 6 )}-${str.substring( 6, 10 )}`;
    }

    return str;
}

/**
 * Strips special characters from the phone number.
 * (321) 456-7890 => 3214567890
 * @param str
 */
export function stripPhoneNumber ( str: string )
{
    if ( !str )
    {
        return '';
    }

    return str.replace( /\D/g, '' );
}

export default {
    asCommaAnd,
    splitCamelCase,
    isNullOrWhitespace,
    isWhitespace,
    isEmpty,
    toTitleCase,
    pluralConditional,
    formatPhoneNumber,
    stripPhoneNumber
};