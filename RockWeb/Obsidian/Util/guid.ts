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
export type Guid = string;

/**
* Generates a new Guid
*/
export function newGuid (): Guid
{
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace( /[xy]/g, ( c ) =>
    {
        const r = Math.random() * 16 | 0;
        const v = c === 'x' ? r : r & 0x3 | 0x8;
        return v.toString( 16 );
    } );
}

/**
 * Returns a normalized Guid that can be compared with string equality (===)
 * @param a
 */
export function normalize ( a: Guid | null )
{
    if ( !a )
    {
        return null;
    }

    return a.toLowerCase();
}

/**
 * Are the guids equal?
 * @param a
 * @param b
 */
export function areEqual ( a: Guid | null, b: Guid | null )
{
    return normalize( a ) === normalize( b );
}

export default {
    newGuid,
    normalize,
    areEqual
};

