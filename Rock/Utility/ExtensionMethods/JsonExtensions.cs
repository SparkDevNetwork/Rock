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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Rock
{
    /// <summary>
    /// JSON Extensions
    /// </summary>
    public static partial class ExtensionMethods
    {
        #region Json Extensions

        /// <summary>
        /// Converts object to JSON string
        /// </summary>
        /// <param name="obj">Object.</param>
        /// <returns></returns>
        public static string ToJson( this object obj )
        {
            return JsonConvert.SerializeObject( obj, Formatting.Indented,
                new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                } );
        }

        /// <summary>
        /// Attempts to deserialize a json string into T.  If it can't be deserialized, returns null
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
            catch
            {
                return default( T );
            }
        }

        #endregion

        #region JObject extension methods

        /// <summary>
        /// Converts a jObject to a dictionary
        /// </summary>
        /// <param name="jobject">The jobject.</param>
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

            arrayKeys.ForEach( k => result[k] = ( (JArray)result[k] ).Values().Select( v => ( (JValue)v ).Value ).ToArray() );
            valueKeys.ForEach( k => result[k] = ToDictionary( result[k] as JObject ) );

            return result;
        }

        #endregion
    }
}
