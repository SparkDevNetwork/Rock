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
using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json.Linq;

namespace Rock
{
    /// <summary>
    /// JSON Extensions
    /// </summary>
    public static partial class ExtensionMethods
    {
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
