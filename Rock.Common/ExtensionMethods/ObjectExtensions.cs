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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Rock
{
    /// <summary>
    /// Extension methods for <see cref="object"/>.
    /// </summary>
    public static class ObjectExtensions
    {
        #region Object Extensions

        /// <summary>
        /// Determines whether the specified value is not null.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if the specified value is not null; otherwise, <c>false</c>.
        /// </returns>
        /// https://github.com/aljazsim/defensive-programming-framework-for-net
        [RockObsolete("1.13.3")]
        [Obsolete("Use the standard object != null instead.")]
        public static bool IsNotNull<T>( this T value )
            where T : class
        {
            return value != null;
        }

        /// <summary>
        /// Determines whether the specified value is null.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if the specified value is null; otherwise, <c>false</c>.
        /// </returns>
        /// https://github.com/aljazsim/defensive-programming-framework-for-net
        [RockObsolete("1.13.3")]
        [Obsolete("Use the standard object == null instead.")]
        public static bool IsNull<T>( this T value ) where T : class
        {
            return value == null;
        }

        /// <summary>
        /// Gets the property Value of the object's property as specified by propertyPathName.
        /// If the object is a dictionary, retrieves the value associated with the matching key.
        /// </summary>
        /// <param name="rootObj">The root obj.</param>
        /// <param name="propertyPathName">Name of the property path.</param>
        /// <returns></returns>
        public static object GetPropertyValue( this object rootObj, string propertyPathName )
        {
            var propPath = propertyPathName.Split( new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries ).ToList<string>();

            object obj = rootObj;
            Type objType = rootObj.GetType();

            while ( propPath.Any() && obj != null )
            {
                if ( obj is IDictionary dictionary )
                {
                    obj = dictionary[propPath.First()];
                }
                else if ( obj is IDictionary<string, object> stringDictionary )
                {
                    obj = stringDictionary[propPath.First()];
                }
                else
                {
                    PropertyInfo property = objType.GetProperty( propPath.First() );
                    if ( property != null )
                    {
                        obj = property.GetValue( obj );
                        objType = property.PropertyType;
                    }
                    else
                    {
                        obj = null;
                    }
                }

                propPath = propPath.Skip( 1 ).ToList();
            }

            return obj;
        }

        /// <summary>
        /// Gets the Property Type of the type's property as specified by propertyPathName.
        /// </summary>
        /// <param name="rootType">Type of the root.</param>
        /// <param name="propertyPathName">Name of the property path.</param>
        /// <returns></returns>
        public static Type GetPropertyType( this Type rootType, string propertyPathName )
        {
            var propPath = propertyPathName.Split( new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries ).ToList<string>();

            Type objType = rootType;
            string elementName = rootType.Name;

            try
            {
                while ( propPath.Any() )
                {
                    elementName = propPath.First();

                    PropertyInfo property = objType.GetProperty( elementName );
                    if ( property != null )
                    {
                        objType = property.PropertyType;
                        propPath = propPath.Skip( 1 ).ToList();
                    }
                    else
                    {
                        objType = null;
                    }
                }

            }
            catch ( Exception )
            {
                throw new Exception( string.Format( "GetPropertyType failed. Could not resolve element \"{0}\" in path \"{1}.{2}\".", elementName, rootType.Name, propertyPathName ) );
            }

            return objType;
        }

        /// <summary>
        /// Safely ToString() this item, even if it's null.
        /// </summary>
        /// <param name="obj">an object</param>
        /// <returns>The ToString or the empty string if the item is null.</returns>
        public static string ToStringSafe( this object obj )
        {
            if ( obj != null )
            {
                return obj.ToString();
            }
            return string.Empty;
        }

        /// <summary>
        /// Returns a string representation of the object or a default value.
        /// </summary>
        /// <param name="obj">an object</param>
        /// <param name="defaultValue"></param>
        /// <returns>A string representation of the object, or the default value if the representation is null or whitespace.</returns>
        public static string ToStringOrDefault( this object obj, string defaultValue )
        {
            if ( obj != null )
            {
                var stringValue = obj.ToString();

                if ( !string.IsNullOrWhiteSpace( stringValue ) )
                {
                    return stringValue;
                }
            }

            return defaultValue;
        }

        /// <summary>
        /// Safely attempt to convert any object to an integer value, or return a default value if unsuccessful.
        /// </summary>
        /// <param name="obj">an object</param>
        /// <param name="defaultValue">the value to return if the conversion is unsuccessful</param>
        /// <returns>The ToString or the empty string if the item is null.</returns>
        public static int ToIntSafe( this object obj, int defaultValue = 0 )
        {
            if ( obj != null )
            {
                int value;

                if ( int.TryParse( obj.ToString(), out value ) )
                {
                    return value;
                }
            }

            return defaultValue;
        }

        /// <summary>
        /// Gets the data annotation attribute from. http://stackoverflow.com/questions/7027613/how-to-retrieve-data-annotations-from-code-programmatically
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance">The instance.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        public static T GetAttributeFrom<T>( this object instance, string propertyName ) where T : System.Attribute
        {
            var attrType = typeof( T );
            var property = instance.GetType().GetProperty( propertyName );
            return ( T ) property.GetCustomAttributes( attrType, false ).First();
        }

        /// <summary>
        /// Converts an object to a string using InvariantCulture and then attempts to parse it to the specified type.
        /// This method helps ensure consistent parsing results across different cultures. 
        /// 
        /// Standard TryParse methods may ignore the supplied CultureInfo.InvariantCulture if the input is an object,
        /// because the object's string representation is generated using the current culture by default.
        /// </summary>
        /// <typeparam name="TOut">The target type to parse to.</typeparam>
        /// <param name="input">The input object to parse.</param>
        /// <param name="output">When this method returns, contains the parsed value if successful, or the default value of <typeparamref name="TOut"/>.</param>
        /// <returns><c>true</c> if parsing was successful; otherwise, <c>false</c>.</returns>
        public static bool TryParseInvariant<TOut>( this object input, out TOut output )
        {
            return input.TryParseInvariant( out output, default( TOut ) );
        }

        /// <summary>
        /// Converts an object to a string using InvariantCulture and then attempts to parse it to the specified type.
        /// This method helps ensure consistent parsing results across different cultures. 
        /// 
        /// Standard TryParse methods may ignore the supplied CultureInfo.InvariantCulture if the input is an object,
        /// because the object's string representation is generated using the current culture by default.
        /// </summary>
        /// <typeparam name="TOut">The target type to parse to.</typeparam>
        /// <param name="input">The input object to parse.</param>
        /// <param name="output">When this method returns, contains the parsed value if successful, or the specified default value.</param>
        /// <param name="defaultValue">The value to assign to <paramref name="output"/> if parsing fails.</param>
        /// <returns><c>true</c> if parsing was successful; otherwise, <c>false</c>.</returns>
        public static bool TryParseInvariant<TOut>( this object input, out TOut output, TOut defaultValue )
        {
            output = defaultValue;

            // We need to convert the object to a string properly, using the InvariantCulture.
            // Otherwise, attempting to use TryParse will used the culture-dependent object-string
            // and ignore the CultureInfo.InvariantCulture we're passing to the TryParse.
            var inputString = Convert.ToString( input, CultureInfo.InvariantCulture );

            Type type = typeof( TOut );
            // Get the four parameter signature of the TryParse:
            // {type}.TryParse( string, NumberStyles, IFormatProvider, out TOut )
            MethodInfo parseMethod = type.GetMethod(
                "TryParse",
                new Type[] { typeof( string ), typeof(NumberStyles), typeof(IFormatProvider), typeof( TOut ).MakeByRefType() } );

            if ( parseMethod != null )
            {
                var numberStyle = NumberStyles.Number;
                if ( type == typeof (int) )
                {
                    numberStyle = NumberStyles.Integer;
                }

                object[] parameters = new object[] { inputString, numberStyle, CultureInfo.InvariantCulture, output };
                var value = parseMethod.Invoke( null, parameters );

                if ( value is bool )
                {
                    bool successful = ( bool ) value;
                    if ( successful )
                    {
                        output = ( TOut ) parameters[3];
                        return true;
                    }
                }
            }
            return false;
        }

        #endregion
    }
}
