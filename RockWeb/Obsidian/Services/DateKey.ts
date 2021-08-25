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

import { toNumberOrNull, zeroPad } from './Number';
const dateKeyLength = 'YYYYMMDD'.length;
const dateKeyNoYearLength = 'MMDD'.length;

/**
 * Gets the year value from the date key.
 * Ex: 20210228 => 2021
 * @param dateKey
 */
export function getYear ( dateKey: string | null )
{
    const defaultValue = 0;

    if ( !dateKey || dateKey.length !== dateKeyLength )
    {
        return defaultValue;
    }

    const asString = dateKey.substring( 0, 4 );
    const year = toNumberOrNull( asString ) || defaultValue;
    return year;
}

/**
 * Gets the month value from the date key.
 * Ex: 20210228 => 2
 * @param dateKey
 */
export function getMonth ( dateKey: string | null )
{
    const defaultValue = 0;

    if ( !dateKey )
    {
        return defaultValue;
    }

    if ( dateKey.length === dateKeyLength )
    {
        const asString = dateKey.substring( 4, 6 );
        return toNumberOrNull( asString ) || defaultValue;
    }

    if ( dateKey.length === dateKeyNoYearLength )
    {
        const asString = dateKey.substring( 0, 2 );
        return toNumberOrNull( asString ) || defaultValue;
    }

    return defaultValue;
}

/**
 * Gets the day value from the date key.
 * Ex: 20210228 => 28
 * @param dateKey
 */
export function getDay ( dateKey: string | null )
{
    const defaultValue = 0;

    if ( !dateKey )
    {
        return defaultValue;
    }

    if ( dateKey.length === dateKeyLength )
    {
        const asString = dateKey.substring( 6, 8 );
        return toNumberOrNull( asString ) || defaultValue;
    }

    if ( dateKey.length === dateKeyNoYearLength )
    {
        const asString = dateKey.substring( 2, 4 );
        return toNumberOrNull( asString ) || defaultValue;
    }

    return defaultValue;
}

/**
 * Gets the datekey constructed from the parts.
 * Ex: (2021, 2, 28) => '20210228'
 * @param year
 * @param month
 * @param day
 */
export function toDateKey ( year: number | null, month: number | null, day: number | null )
{
    if ( !year || year > 9999 || year < 0 )
    {
        year = 0;
    }

    if ( !month || month > 12 || month < 0 )
    {
        month = 0;
    }

    if ( !day || day > 31 || day < 0 )
    {
        day = 0;
    }

    const yearStr = zeroPad( year, 4 );
    const monthStr = zeroPad( month, 2 );
    const dayStr = zeroPad( day, 2 );

    return `${yearStr}${monthStr}${dayStr}`;
}

/**
 * Gets the datekey constructed from the parts.
 * Ex: (2, 28) => '0228'
 * @param month
 * @param day
 */
export function toNoYearDateKey (month: number | null, day: number | null )
{
    if ( !month || month > 12 || month < 0 )
    {
        month = 0;
    }

    if ( !day || day > 31 || day < 0 )
    {
        day = 0;
    }

    const monthStr = zeroPad( month, 2 );
    const dayStr = zeroPad( day, 2 );

    return `${monthStr}${dayStr}`;
}

export default {
    getYear,
    getMonth,
    getDay,
    toDateKey,
    toNoYearDateKey
};