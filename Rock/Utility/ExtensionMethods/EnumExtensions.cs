// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.ComponentModel;

namespace Rock
{
    /// <summary>
    /// System.Enum extensions
    /// </summary>
    public static class EnumExtensions
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
        /// Gets the enum description.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static string GetDescription( this Enum value )
        {
            var type = value.GetType();
            string name = Enum.GetName( type, value );
            if ( name != null )
            {
                System.Reflection.FieldInfo field = type.GetField( name );
                if ( field != null )
                {
                    var attr = System.Attribute.GetCustomAttribute( field,
                        typeof( DescriptionAttribute ) ) as DescriptionAttribute;
                    if ( attr != null )
                    {
                        return attr.Description;
                    }
                }
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
        /// Converts a string value to an enum value.
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
        /// Converts to enum or null.
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

        #endregion Enum Extensions
    }
}
