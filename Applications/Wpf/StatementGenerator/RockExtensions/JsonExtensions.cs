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
using System.IO;
using System.Linq;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Rock
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
        /// Converts object to JSON string with an option to ignore errors
        /// </summary>
        /// <param name="obj">Object.</param>
        /// <param name="format">The format.</param>
        /// <param name="ignoreErrors">if set to <c>true</c> [ignore errors].</param>
        /// <returns></returns>
        public static string ToJson( this object obj, Formatting format)
        {
            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Formatting = format
            };


            return JsonConvert.SerializeObject( obj, settings );
        }

        /// <summary>
        /// Converts object to JSON string and saves directly to a file
        /// </summary>
        /// <param name="obj">Object.</param>
        /// <param name="format">The format.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="excludeDefaultValues">if set to <c>true</c> [exclude default values].</param>
        public static void ToJsonFile( this object obj, Formatting format, string fileName, bool excludeDefaultValues )
        {
            string json;
            if ( excludeDefaultValues )
            {
                // this can cut the size in half, but it won't show properties that have default value (bool false, string null, etc)
                var settings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    DefaultValueHandling = DefaultValueHandling.Ignore,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    Formatting = format
                };

                json = JsonConvert.SerializeObject( obj, settings );
            }
            else
            {
                json = obj.ToJson( format );
            }

            File.WriteAllText( fileName, json );
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
                return val.FromJsonOrThrow<T>();
            }
            catch ( Exception ex )
            {
                System.Diagnostics.Debug.WriteLine( ex.Message );
                return default( T );
            }
        }

        /// <summary>
        /// Attempts to deserialize a JSON string into T. If it can't be deserialized, it will throw an exception
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="val">The value.</param>
        /// <returns></returns>
        public static T FromJsonOrThrow<T>( this string val )
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
                throw new Exception( $"Unable to deserialize to {typeof( T ).Name}. {ex}", ex );
            }
        }

        

        #endregion
    }
}
