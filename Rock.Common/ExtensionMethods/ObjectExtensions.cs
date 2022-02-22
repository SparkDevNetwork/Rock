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
        public static bool IsNotNull<T>( this T value )
            where T : class
        {
            return !value.IsNull();
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

        #endregion
    }
}
