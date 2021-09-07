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

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Rock
{
    /// <summary>
    /// JSON Extensions
    /// </summary>
    public static partial class ExtensionMethods
    {
        #region JSON Extensions

        /// <summary>
        /// Converts object to JSON string.
        /// </summary>
        /// <remarks>
        /// Public properties are serialized, but public fields are ignored.
        /// </remarks>
        /// <param name="obj">Object.</param>
        /// <returns></returns>
        [RockObsolete( "1.13" )]
        [Obsolete( "Use the extension methods in the Rock.Common assembly instead." )]
        public static string ToJson( object obj )
        {
            return JsonConvert.SerializeObject( obj, Formatting.None,
                new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                } );
        }

        /// <summary>
        /// Converts object to JSON string
        /// </summary>
        /// <param name="obj">Object.</param>
        /// <param name="format">The format.</param>
        /// <returns></returns>
        [RockObsolete( "1.13" )]
        [Obsolete( "Use the extension methods in the Rock.Common assembly instead." )]
        public static string ToJson( object obj, Formatting format )
        {
            return ToJson( obj, format, false );
        }

        /// <summary>
        /// Converts object to JSON string with an option to ignore errors
        /// </summary>
        /// <param name="obj">Object.</param>
        /// <param name="format">The format.</param>
        /// <param name="ignoreErrors">if set to <c>true</c> [ignore errors].</param>
        /// <returns></returns>
        [RockObsolete( "1.13" )]
        [Obsolete( "Use the extension methods in the Rock.Common assembly instead." )]
        public static string ToJson( object obj, Formatting format, bool ignoreErrors )
        {
            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Formatting = format
            };

            if ( ignoreErrors )
            {
                settings.Error += new EventHandler<Newtonsoft.Json.Serialization.ErrorEventArgs>( ( s, e ) =>
                {
                    e.ErrorContext.Handled = true;
                } );
            }

            return JsonConvert.SerializeObject( obj, settings );
        }

        /// <summary>
        /// Attempts to deserialize a JSON string into T.  If it can't be deserialized, returns null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="val">The value.</param>
        /// <returns></returns>
        [RockObsolete( "1.13" )]
        [Obsolete( "Use the extension methods in the Rock.Common assembly instead." )]
        public static T FromJsonOrNull<T>( string val )
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
        [RockObsolete( "1.13" )]
        [Obsolete( "Use the extension methods in the Rock.Common assembly instead." )]
        public static T FromJsonOrThrow<T>( string val )
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

        /// <summary>
        /// Attempts to deserialize a JSON string into either a <see cref="ExpandoObject" /> or a list of <see cref="ExpandoObject" />.  If it can't be deserialized, return null
        /// </summary>
        /// <param name="val">The value.</param>
        /// <returns></returns>
        [RockObsolete( "1.13" )]
        [Obsolete( "Use the extension methods in the Rock.Common assembly instead." )]
        public static object FromJsonDynamicOrNull( string val )
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
        [RockObsolete( "1.13" )]
        [Obsolete( "Use the extension methods in the Rock.Common assembly instead." )]
        public static object FromJsonDynamic( string val )
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
    }
}
