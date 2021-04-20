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
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Rock.Common
{
    /// <summary>
    /// JSON Extensions
    /// </summary>
    public static partial class ExtensionMethods
    {
        #region JSON Extensions

        /// <summary>
        /// Converts object to JSON string
        /// </summary>
        /// <param name="obj">Object.</param>
        /// <returns></returns>
        public static string ToJson( this object obj )
        {
            return JsonConvert.SerializeObject( obj, Formatting.None,
                new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                } );
        }

        /// <summary>
        /// To the JSON.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="format">The format.</param>
        /// <returns></returns>
        public static string ToJson( this object obj, Formatting format )
        {
            return JsonConvert.SerializeObject( obj, format,
                new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                } );
        }

        /// <summary>
        /// Attempts to deserialize a JSON string into T.  If it can't be deserialized, returns null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="val">The value.</param>
        /// <returns></returns>
        public static T FromJsonOrNull<T>( this string val )
        {
            try
            {
                if ( string.IsNullOrWhiteSpace( val ) )
                {
                    return default( T );
                }
                else
                {
                    return JsonConvert.DeserializeObject<T>( val );
                }
            }
            catch ( Exception ex )
            {
                System.Diagnostics.Debug.WriteLine( $"Unable to deserialize to {typeof( T ).Name}. {ex}" );
                return default( T );
            }
        }

        /// <summary>
        /// Attempts to deserialize a JSON string into either a <see cref="ExpandoObject" /> or a list of <see cref="ExpandoObject" />.  If it can't be deserialized, return null
        /// </summary>
        /// <param name="val">The value.</param>
        /// <returns></returns>
        public static object FromJsonDynamicOrNull( this string val )
        {
            try
            {
                return val.FromJsonDynamic();
            }
            catch ( Exception ex )
            {
                System.Diagnostics.Debug.WriteLine( $"Unable to deserialize to dynamic. {ex}" );
                return null;
            }
        }

        /// <summary>
        /// Attempts to deserialize a JSON string into either a <see cref="ExpandoObject" /> or a list of <see cref="ExpandoObject" />. If it can't be deserialized, throws an exception
        /// </summary>
        /// <param name="val">The value.</param>
        /// <returns></returns>
        public static object FromJsonDynamic( this string val )
        {
            var converter = new ExpandoObjectConverter();
            object dynamicObject = null;

            // keep track of which exception most applies. 
            Exception singleObjectException = null;
            Exception arrayObjectException = null;

            try
            {
                // first try to deserialize as straight ExpandoObject
                dynamicObject = JsonConvert.DeserializeObject<ExpandoObject>( val, converter );
            }
            catch ( Exception firstException )
            {
                try
                {
                    singleObjectException = firstException;
                    dynamicObject = JsonConvert.DeserializeObject<List<ExpandoObject>>( val, converter );

                }
                catch ( Exception secondException )
                {
                    try
                    {
                        arrayObjectException = secondException;

                        // if it didn't deserialize as a List of ExpandoObject, try it as a List of plain objects
                        dynamicObject = JsonConvert.DeserializeObject<List<object>>( val, converter );
                    }
                    catch
                    {
                        // if both the attempt to deserialize an object and an object list fail, it probably isn't valid JSON, so throw the singleObjectException
                        if ( singleObjectException != null )
                        {
                            throw singleObjectException;
                        }
                        else
                        {
                            throw arrayObjectException;
                        }
                    }
                }
            }

            return dynamicObject;
        }

        #endregion

        #region JObject extension methods

        /// <summary>
        /// Converts a jObject to a dictionary
        /// </summary>
        /// <param name="jobject">The JObject.</param>
        /// <returns></returns>
        public static IDictionary<string, object> ToDictionary( this JObject jobject )
        {
            var result = jobject.ToObject<Dictionary<string, object>>();

            var valueKeys = result
                .Where( r => r.Value != null && r.Value.GetType() == typeof( JObject ) )
                .Select( r => r.Key )
                .ToList();

            var arrayKeys = result
                .Where( r => r.Value != null && r.Value.GetType() == typeof( JArray ) )
                .Select( r => r.Key )
                .ToList();

            arrayKeys.ForEach( k => result[k] = ( ( JArray ) result[k] ).ToObjectArray() );
            valueKeys.ForEach( k => result[k] = ToDictionary( result[k] as JObject ) );

            return result;
        }

        /// <summary>
        /// Converts a JArray to a Object array
        /// </summary>
        /// <param name="jarray">The JArray.</param>
        /// <returns></returns>
        public static object[] ToObjectArray( this JArray jarray )
        {
            var valueList = new List<object>();

            for ( var i = 0; i < jarray.Count; i++ )
            {
                var obj = jarray[i];
                if ( obj.GetType() == typeof( JObject ) )
                {
                    valueList.Add( ( ( JObject ) obj ).ToDictionary() );
                }

                if ( obj.GetType() == typeof( JValue ) )
                {
                    valueList.Add( ( ( JValue ) obj ).Value );
                }
            }

            return valueList.ToArray();
        }

        #endregion
    }
}
