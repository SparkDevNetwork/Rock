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
using System;

namespace Rock
{
    /// <summary>
    ///
    /// </summary>
    public static partial class ExtensionMethods
    {
        /// <summary>
        /// Determines whether [is null or zero].
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if [is null or zero] [the specified value]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNullOrZero( this int? value )
        {
            return ( value ?? 0 ) == 0;
        }

        /// <summary>
        /// Determines whether [is not null or zero].
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if [is not null or zero] [the specified value]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNotNullOrZero( this int? value )
        {
            return !IsNullOrZero( value );
        }

        /// <summary>
        /// Returns a formated string of the memory size.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="unit">The unit.</param>
        /// <param name="decimalPlaces">The decimal places.</param>
        /// <returns></returns>
        public static string FormatAsSpecificMemorySize( this int value, MemorySizeUnit unit, int decimalPlaces = 0 )
        {
            var size = ( value / ( double ) Math.Pow( 1024, ( Int64 ) unit ) );

            return string.Format( "{0:n" + decimalPlaces + "} {1}",
                size,
                unit );
        }

        /// <summary>
        /// Units of measure for memory from bytes to YottaBytes
        /// </summary>
        public enum MemorySizeUnit
        {
            /// <summary>
            /// Bytes
            /// </summary>
            Bytes,

            /// <summary>
            /// KiloBytes
            /// </summary>
            KB,

            /// <summary>
            /// MegaBytes
            /// </summary>
            MB,

            /// <summary>
            /// GigaBytes
            /// </summary>
            GB,

            /// <summary>
            /// TeraBytes
            /// </summary>
            TB,

            /// <summary>
            /// PetaBytes
            /// </summary>
            PB,

            /// <summary>
            /// ExaBytes
            /// </summary>
            EB,

            /// <summary>
            /// ZettaBytes
            /// </summary>
            ZB,

            /// <summary>
            /// YottaBytes
            /// </summary>
            YB
        }

        /// <summary>
        /// Returns a formatted string of the memory size.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="decimalPlaces">The decimal places.</param>
        /// <returns></returns>
        public static string FormatAsMemorySize( this int value, int decimalPlaces = 0 )
        {
            if ( decimalPlaces < 0 )
            {
                throw new ArgumentOutOfRangeException( "decimalPlaces" );
            }

            if ( value < 0 )
            {
                return "-" + FormatAsMemorySize( -value );
            }

            if ( value == 0 )
            {
                return string.Format( "{0:n" + decimalPlaces + "} bytes", 0 );
            }

            // mag is 0 for bytes, 1 for KB, 2, for MB, etc.
            int mag = ( int ) Math.Log( value, 1024 );

            // 1L << (mag * 10) == 2 ^ (10 * mag) 
            // [i.e. the number of bytes in the unit corresponding to mag]
            decimal adjustedSize = ( decimal ) value / ( 1L << ( mag * 10 ) );

            // make adjustment when the value is large enough that
            // it would round up to 1000 or more
            if ( Math.Round( adjustedSize, decimalPlaces ) >= 1000 )
            {
                mag += 1;
                adjustedSize /= 1024;
            }

            return string.Format( "{0:n" + decimalPlaces + "} {1}",
                adjustedSize,
                SizeSuffixes[mag] );
        }

        /// <summary>
        /// Returns a formatted string of the memory size.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="decimalPlaces">The decimal places.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">decimalPlaces</exception>
        public static string FormatAsMemorySize( this long value, int decimalPlaces = 0 )
        {
            if ( decimalPlaces < 0 )
            {
                throw new ArgumentOutOfRangeException( "decimalPlaces" );
            }

            if ( value < 0 )
            {
                return "-" + FormatAsMemorySize( -value );
            }

            if ( value == 0 )
            {
                return string.Format( "{0:n" + decimalPlaces + "} bytes", 0 );
            }

            // mag is 0 for bytes, 1 for KB, 2, for MB, etc.
            int mag = ( int ) Math.Log( value, 1024 );

            // 1L << (mag * 10) == 2 ^ (10 * mag) 
            // [i.e. the number of bytes in the unit corresponding to mag]
            decimal adjustedSize = ( decimal ) value / ( 1L << ( mag * 10 ) );

            // make adjustment when the value is large enough that
            // it would round up to 1000 or more
            if ( Math.Round( adjustedSize, decimalPlaces ) >= 1000 )
            {
                mag += 1;
                adjustedSize /= 1024;
            }

            return string.Format( "{0:n" + decimalPlaces + "} {1}",
                adjustedSize,
                SizeSuffixes[mag] );
        }

        /// <summary>
        /// Returns a formatted string of the duration.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>A friendly time duration such as '1h 44m' or '5m 22s'</returns>
        public static string ToFriendlyDuration( this int? value )
        {
            if ( !value.HasValue )
            {
                return string.Empty;
            }

            var totalSeconds = value;
            // Format the results
            TimeSpan timeSpan = TimeSpan.FromSeconds( value.Value );
            var formattedTime = string.Empty;
            if ( timeSpan.Days != default( int ) )
            {
                totalSeconds -= timeSpan.Days * 24 * 60 * 60;
                formattedTime = string.Format( "{0}d ", timeSpan.Days );
            }

            if ( timeSpan.Hours != default( int ) || ( totalSeconds > 0 && formattedTime.IsNotNullOrWhiteSpace() ) )
            {
                totalSeconds -= timeSpan.Hours * 60 * 60;
                formattedTime += string.Format( "{0}h ", timeSpan.Hours );
            }

            if ( timeSpan.Minutes != default( int ) || ( totalSeconds > 0 && formattedTime.IsNotNullOrWhiteSpace() ) )
            {
                totalSeconds -= timeSpan.Minutes * 60;
                formattedTime += string.Format( "{0}m ", timeSpan.Minutes );
            }

            if ( timeSpan.Seconds != default( int ) )
            {
                formattedTime += string.Format( "{0}s", timeSpan.Seconds );
            }

            return formattedTime;
        }

        /// <summary>
        /// Memory size suffixes
        /// </summary>
        static readonly string[] SizeSuffixes = { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
    }
}