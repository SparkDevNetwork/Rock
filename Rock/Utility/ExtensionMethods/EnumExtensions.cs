﻿// <copyright>
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Rock
{
    /// <summary>
    /// System.Enum extensions
    /// </summary>
    public static partial class ExtensionMethods
    {
        #region Enum Extensions

        /// <summary>
        /// Converts to the enum value to its string value.
        /// </summary>
        /// <param name="eff">The eff.</param>
        /// <param name="SplitCase">if set to <c>true</c> [split case].</param>
        /// <returns></returns>
        public static String ConvertToString( this Enum eff, bool SplitCase = true )
        {
            if ( SplitCase )
            {
                return Enum.GetName( eff.GetType(), eff ).SplitCase();
            }
            else
            {
                return Enum.GetName( eff.GetType(), eff );
            }
        }

        /// <summary>
        /// Gets the <see cref="System.Attribute"/>s of the specified type.
        /// </summary>
        /// <typeparam name="TAttribute">The type of the <see cref="System.Attribute"/>.</typeparam>
        /// <param name="value">The value.</param>
        /// <returns>The <see cref="System.Attribute"/>s of the specified type, or an empty array if <paramref name="value"/> is a valid Enum but no matching <see cref="System.Attribute"/>s are found, or <see langword="null"/> if <paramref name="value"/> is not a valid Enum.</returns>
        public static TAttribute[] GetAttributes<TAttribute>( this Enum value ) where TAttribute : class
        {
            TAttribute[] attrs = null;

            var type = value.GetType();
            string name = Enum.GetName( type, value );
            if ( name != null )
            {
                System.Reflection.FieldInfo field = type.GetField( name );
                if ( field != null )
                {
                    attrs = System.Attribute.GetCustomAttributes( field,
                        typeof( TAttribute ) ) as TAttribute[];
                }
            }

            return attrs;
        }

        /// <summary>
        /// Gets the first <see cref="System.Attribute"/> of the specified type.
        /// </summary>
        /// <typeparam name="TAttribute">The type of the <see cref="System.Attribute"/>.</typeparam>
        /// <param name="value">The value.</param>
        /// <returns>The first <see cref="System.Attribute"/> of the specified type, or <see langword="null"/> if <paramref name="value"/> is not a valid Enum or no matching <see cref="System.Attribute"/>s are found.</returns>
        public static TAttribute GetAttribute<TAttribute>( this Enum value ) where TAttribute : class
        {
            return GetAttributes<TAttribute>( value )?.FirstOrDefault();
        }

        /// <summary>
        /// Gets the enum description.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static string GetDescription( this Enum value )
        {
            var attr = GetAttribute<DescriptionAttribute>( value );

            if ( attr != null )
            {
                return attr.Description;
            }

            return null;
        }

        /// <summary>
        /// Converts to int.
        /// </summary>
        /// <param name="eff">The eff.</param>
        /// <returns></returns>
        public static int ConvertToInt( this Enum eff )
        {
            return Convert.ToInt32( eff );
        }

        /// <summary>
        /// Converts a string value to an enum value, first using a case-sensitive match, but also trying a case-insensitive match
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumValue">The enum value.</param>
        /// <param name="defaultValue">The default value to use if the value cannot be parsed. Leave null to throw an exception if the value cannot be parsed. </param>
        /// <returns></returns>
        public static T ConvertToEnum<T>( this string enumValue, T? defaultValue = null ) where T : struct // actually limited to enum, but struct is the closest we can do
        {
            T? result = ConvertToEnumOrNull<T>( enumValue, defaultValue );
            if ( result.HasValue )
            {
                return result.Value;
            }
            else
            {
                throw new Exception( string.Format( "'{0}' is not a member of the {1} enumeration.", enumValue, typeof( T ).Name ) );
            }
        }

        /// <summary>
        /// Converts a string to an enum value, first using a case-sensitive match, but also trying a case-insensitive match
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumValue">The enum value.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public static T? ConvertToEnumOrNull<T>( this string enumValue, T? defaultValue = null ) where T : struct // actually limited to enum, but struct is the closest we can do
        {
            T result;
            if ( Enum.TryParse<T>( ( enumValue ?? "" ).Replace( " ", "" ), out result ) && Enum.IsDefined( typeof( T ), result ) )
            {
                // a regular case-sensitive parse found it, so we got it
                return result;
            }
            else if ( Enum.TryParse<T>( ( enumValue ?? "" ).Replace( " ", "" ), true, out result ) && Enum.IsDefined( typeof( T ), result ) )
            {
                // case-sensitive parse didn't work, but parsing again with ignoreCase = true got it for us
                return result;
            }
            else
            {
                if ( defaultValue.HasValue )
                {
                    return defaultValue.Value;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Gets the individual enum values of a Flags enumeration value.
        /// </summary>
        /// <typeparam name="T">The Enum type of enumValue.</typeparam>
        /// <param name="enumValue">The enum value whose flags should be retrieved.</param>
        /// <returns>An enumerable collection of the individual flag values.</returns>
        public static IEnumerable<T> GetFlags<T>( this Enum enumValue )
        {
            foreach ( var value in Enum.GetValues( enumValue.GetType() ).Cast<T>() )
            {
                Enum flag = ( Enum ) Enum.Parse( typeof( T ), value.ToString() );

                if ( enumValue.HasFlag( flag ) )
                {
                    yield return value;
                }
            }
        }

        #endregion Enum Extensions
    }
}
